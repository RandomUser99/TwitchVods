using Ardalis.GuardClauses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        private const int ObjectCountLimit = 100;

        private static string BaseUrl => "https://api.twitch.tv/helix";
        private TwitchAuthToken _token;

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

            request.Headers.Add("Client-ID", _settings.TwitchApiClientId);
            request.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");

            return request;
        }

        public async Task<Channel> ChannelVideosAsync()
        {
            var channel = Channel.Create(_channelName);

            var retrievedVideos = await _retryRetryPolicy.ExecuteAsync(async () => await GetVideosAsync(_settings.LimitVideos));

            foreach (var video in retrievedVideos)
            {
                await _retryRetryPolicy.ExecuteAsync(async () =>
                {
                    await PopulateMarkersAsync(video);
                });
            }

            channel.AddVideoRange(retrievedVideos);

            return channel;
        }

        private async Task<List<Video>> GetVideosAsync(bool limitVideos)
        {
            var videos = new List<Video>();
            var cursor = string.Empty;

            do
            {
                var apiEndpoint = $"{BaseUrl}/videos?user_id={_channelUserId}&type=archive&period=all&after={cursor}&first={ObjectCountLimit}"; ;
                var request = CreateWebRequest(apiEndpoint);

                var webResponse = await request.GetResponseAsync();
                using var reader = new StreamReader(webResponse.GetResponseStream());

                var readerOutput = await reader.ReadToEndAsync();

                var response = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<VideoResponse>(readerOutput));
                var videosData = response.Videos.Select(Video.FromVideoData);
                videos.AddRange(videosData);
                cursor = response.Pagination.Cursor;

                Console.WriteLine("{0} Retrieved: {1}", _channelName, videos.Count);

                if (limitVideos || videosData.Count() < ObjectCountLimit)
                {
                    break;
                }

            } while (!string.IsNullOrEmpty(cursor));

            return videos;
        }

        private async Task PopulateMarkersAsync(Video video)
        {
            var apiEndpoint = $"{BaseUrl}/streams/markers?video_ID={video.Id}&first={ObjectCountLimit}";

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
        }
    }
}
