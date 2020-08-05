using Ardalis.GuardClauses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TwitchVods.Core.Twitch.Clients
{
    using Models;

    internal class HelixTwitchClient : ITwitchClient
    {
        private readonly string _channelName;
        private readonly Settings _settings;
        private string _channelUserId;
        private readonly IAsyncPolicy _retryRetryPolicy;

        private static string BaseUrl => "https://api.twitch.tv/helix";
        private TwitchAuthToken _token;

        private string GetChannelVideosEndpoint(string after)
        {
            return $"{BaseUrl}/videos?user_id={_channelUserId}&type=archive&period=all&after={after}";
            //return $"{BaseUrl}/channels/{_channelUserId}/videos?broadcast_type=archive&limit={limit}&offset={offset}";
        }

        public HelixTwitchClient(string channelName, Settings settings, IAsyncPolicy retryPolicy)
        {
            Guard.Against.NullOrEmpty(channelName, nameof(channelName));
            Guard.Against.Null(settings, nameof(settings));
            Guard.Against.Null(retryPolicy, nameof(retryPolicy));

            _channelName = channelName;
            _settings = settings;
            _retryRetryPolicy = retryPolicy;

            Init();
        }

        private void Init()
        {
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
        }

        private void SetChannelUserId()
        {
            if (!string.IsNullOrWhiteSpace(_channelUserId))
                return;

            var apiEndpoint = $"{BaseUrl}/users?login={_channelName}";

            var request = CreateWebRequest(apiEndpoint);

            var webResponse = request.GetResponse();

            using var reader = new StreamReader(webResponse.GetResponseStream());

            dynamic jsonData = JObject.Parse(reader.ReadToEnd());
            _channelUserId = jsonData["data"][0]["id"].ToString();
        }

        private HttpWebRequest CreateWebRequest(string apiEndpoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(apiEndpoint);

            // Need to specify the client ID https://dev.twitch.tv/docs/api#step-1-setup
            request.Headers.Add("Client-ID", _settings.TwitchApiClientId);
            request.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");

            return request;
        }

        //private async Task<int> GetTotalVideoCountAsync()
        //{
        //    // Only need one vod to get the total available
        //    const int limit = 1;
        //    const int offset = 0;

        //    var apiEndpoint = GetChannelVideosEndpoint(limit, offset);
        //    var request = CreateWebRequest(apiEndpoint);
        //    var webResponse = await request.GetResponseAsync();
        //    int count;

        //    using var reader = new StreamReader(webResponse.GetResponseStream());

        //    var readerOutput = await reader.ReadToEndAsync();
        //    dynamic jsonData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(readerOutput));

        //    count = jsonData._total;

        //    return count;
        //}

        public async Task<Channel> ChannelVideosAsync()
        {
            var channel = Channel.Create(_channelName);

            //  var totalVideos = await _retryRetryPolicy.ExecuteAsync(async () => await GetTotalVideoCountAsync());

            //const int limit = 50;

            do
            {
                var retrievedVideos =
                    await _retryRetryPolicy.ExecuteAsync(async () => await GetVideosAsync(_settings.LimitVideos));

                foreach (var video in retrievedVideos)
                {
                    await _retryRetryPolicy.ExecuteAsync(async () =>
                    {
                        await PopulateMarkersAsync(video);
                    });
                }


                channel.AddVideoRange(retrievedVideos);

            } while (true);

            return channel;
        }

        private async Task<List<Video>> GetVideosAsync(bool limitVideos)
        {
            var videos = new List<Video>();
            var cursor = string.Empty;

            do
            {
                var apiEndpoint = GetChannelVideosEndpoint(cursor);
                var request = CreateWebRequest(apiEndpoint);

                var webResponse = await request.GetResponseAsync();
                using var reader = new StreamReader(webResponse.GetResponseStream());

                var readerOutput = await reader.ReadToEndAsync();

                var response = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<VideoResponse>(readerOutput));
                videos.AddRange(response.Videos.Select(Video.FromVideoData));
                cursor = response.Pagination.Cursor;

                Console.WriteLine("{0} Retrieved: {1}", _channelName, videos.Count);

                if (limitVideos)
                {
                    break;
                }

            } while (!string.IsNullOrEmpty(cursor));

            return videos;
        }

        private async Task PopulateMarkersAsync(Video video)
        {
            var apiEndpoint = GetVideoMarkersEndpoint(video.Id);

            var request = CreateWebRequest(apiEndpoint);

            var webResponse = await request.GetResponseAsync();

            MarkerResponse response;
            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var readerOutput = await reader.ReadToEndAsync();
                response = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<MarkerResponse>(readerOutput));
            }

            if (response.markers.game_changes == null)
                return;

            foreach (var marker in response.markers.game_changes)
            {
                video.AddMarker(Marker.Create(marker.label, marker.time));
            }

            static string GetVideoMarkersEndpoint(string videoId)
            {
                return $"{BaseUrl}/videos/{videoId}/markers?api_version=5";
            }
        }
    }
}
