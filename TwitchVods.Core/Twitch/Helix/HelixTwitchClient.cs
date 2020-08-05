using Ardalis.GuardClauses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchVods.Core.Twitch.Kraken;

namespace TwitchVods.Core.Twitch.Helix
{
    using Models;

    internal enum TwitchApiVersion
    {
        v3 = 3,
        v5 = 5
    }

    internal class TwitchAuthToken
    {
        public string AccessToken { get; private set; }
        public int ExpiresInSeconds { get; private set; }
        public DateTime DateCreated { get; private set; }
        public bool HasExpired => DateTime.Now >= DateCreated.AddSeconds(ExpiresInSeconds);

        public static TwitchAuthToken Create(string token, int expiresIn)
        {
            return new TwitchAuthToken
            {
                AccessToken = token,
                ExpiresInSeconds = expiresIn,
                DateCreated = DateTime.Now
            };
        }
    }

    internal class HelixTwitchClient : ITwitchClient
    {
        private readonly string _channelName;
        private readonly Settings _settings;
        private string _channelUserId;
        private readonly IAsyncPolicy _retryRetryPolicy;

        private static string BaseUrl => "https://api.twitch.tv/helix";
        private TwitchAuthToken _token;

        private string GetChannelVideosEndpoint(int limit, int offset)
        {
            return $"{BaseUrl}/videos?user_id={_settings}&type=archive&period=all";
            //return $"{BaseUrl}/channels/{_channelUserId}/videos?broadcast_type=archive&limit={limit}&offset={offset}";
        }

        private static string GetVideoMarkersEndpoint(string videoId)
        {
            return $"{BaseUrl}/videos/{videoId}/markers?api_version=5";
        }

        public HelixTwitchClient(string channelName, Settings settings, IAsyncPolicy retryPolicy)
        {
            Guard.Against.NullOrEmpty(channelName, nameof(channelName));
            Guard.Against.Null(settings, nameof(settings));
            Guard.Against.Null(retryPolicy, nameof(retryPolicy));

            _channelName = channelName;
            _settings = settings;
            _retryRetryPolicy = retryPolicy;

            Authenticate();
            SetChannelUserId();
        }

        private void Authenticate()
        {
            var apiEndpoint = $"https://id.twitch.tv/oauth2/token?client_id={_settings.TwitchApiClientId}&client_secret={_settings.TwitchApiClientSecret}&grant_type=client_credentials";

            var request = (HttpWebRequest)WebRequest.Create(apiEndpoint);
            request.Method = "POST";

            var webResponse = request.GetResponse();

            using var reader = new StreamReader(webResponse.GetResponseStream());

            dynamic jsonData = JObject.Parse(reader.ReadToEnd());

            _token = TwitchAuthToken.Create(jsonData["access_token"].ToString(), int.Parse(jsonData["expires_in"].ToString()));
            _channelUserId = jsonData["users"][0]["_id"].ToString();
        }

        private void SetChannelUserId()
        {
            if (!string.IsNullOrWhiteSpace(_channelUserId))
                return;

            var apiEndpoint = $"{BaseUrl}/users?login={_channelName}";

            var request = CreateWebRequest(apiEndpoint, TwitchApiVersion.v5);

            var webResponse = request.GetResponse();

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                dynamic jsonData = JObject.Parse(reader.ReadToEnd());
                _channelUserId = jsonData["users"][0]["_id"].ToString();
            }
        }

        private HttpWebRequest CreateWebRequest(string apiEndpoint, TwitchApiVersion apiVersion)
        {
            var request = (HttpWebRequest)WebRequest.Create(apiEndpoint);

            // Need to specify the version of the API to use.
            //request.Accept = $@"application/vnd.twitchtv.{apiVersion}+json";

            // Need to specify the client ID https://dev.twitch.tv/docs/api#step-1-setup
            request.Headers.Add("Client-ID", _settings.TwitchApiClientId);

            return request;
        }

        private async Task<int> GetTotalVideoCountAsync()
        {
            // Only need one vod to get the total available
            const int limit = 1;
            const int offset = 0;

            var apiEndpoint = GetChannelVideosEndpoint(limit, offset);
            var request = CreateWebRequest(apiEndpoint, TwitchApiVersion.v5);
            var webResponse = await request.GetResponseAsync();
            int count;

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var readerOutput = await reader.ReadToEndAsync();
                dynamic jsonData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(readerOutput));

                count = jsonData._total;
            }

            return count;
        }

        public async Task<Channel> ChannelVideosAsync()
        {
            var channel = Channel.Create(_channelName);

            var totalVideos = await _retryRetryPolicy.ExecuteAsync(async () => await GetTotalVideoCountAsync());

            const int limit = 50;

            for (var offset = 0; offset < totalVideos; offset += limit)
            {
                var retrievedVideos = await _retryRetryPolicy.ExecuteAsync(async () => await GetVideosAsync(limit, offset));

                foreach (var video in retrievedVideos)
                {
                    await _retryRetryPolicy.ExecuteAsync(async () =>
                   {
                       await PopulateMarkersAsync(video);
                   });
                }

                if (retrievedVideos.Count > limit)
                    throw new ApplicationException(
                        "Twitch API is returning bad data. It is returning more records than the limit value.");

                channel.AddVideoRange(retrievedVideos);

                Console.WriteLine("{0} Retrieved: {1} of {2} ", _channelName, channel.TotalVideoCount, totalVideos);

                // If LimitVideos is true, it will bail out. Used for testing purposes.
                if (_settings.LimitVideos)
                    break;
            }
            return channel;
        }

        private async Task<List<Video>> GetVideosAsync(int limit, int offset = 0)
        {
            var apiEndpoint = GetChannelVideosEndpoint(limit, offset);

            var request = CreateWebRequest(apiEndpoint, TwitchApiVersion.v5);

            var webResponse = await request.GetResponseAsync();
            var videos = new List<Video>();

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var readerOutput = await reader.ReadToEndAsync();
                var response = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<KrakenVideoResponse>(readerOutput));

                foreach (var data in response.videos)
                {
                    var id = Regex.Replace(data._id, "[^0-9]+", string.Empty);

                    DateTime.TryParse(data.created_at.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateValue);


                    var broadcastId = long.Parse(data.broadcast_id.ToString());
                    var video = Video.Create(id, data.title, broadcastId, dateValue, data.game, data.length, data.url, data.views);
                    videos.Add(video);
                }
            }
            return videos;
        }

        private async Task PopulateMarkersAsync(Video video)
        {
            var apiEndpoint = GetVideoMarkersEndpoint(video.Id);

            var request = CreateWebRequest(apiEndpoint, TwitchApiVersion.v5);

            var webResponse = await request.GetResponseAsync();

            KrakenMarkerResponse response;
            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var readerOutput = reader.ReadToEnd();
                response = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<KrakenMarkerResponse>(readerOutput));
            }

            if (response.markers.game_changes == null)
                return;

            foreach (var marker in response.markers.game_changes)
            {
                video.AddMarker(Marker.Create(marker.label, marker.time));
            }
        }
    }
}
