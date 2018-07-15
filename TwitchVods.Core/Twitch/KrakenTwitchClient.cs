using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using TwitchVods.Core.Models;

namespace TwitchVods.Core.Twitch
{
    internal enum TwitchApiVersion
    {
        v3 = 3,
        v5 = 5
    }

    internal class KrakenTwitchClient : ITwitchClient
    {
        private readonly string _channelName;
        private readonly Settings _settings;
        private string _channelUserId;
        private readonly RetryPolicy _retryPolicy;
        private const int MaxRetries = 10;

        private static string BaseUrl => "https://api.twitch.tv/kraken";

        private string GetChannelVideosEndpoint(int limit, int offset)
        {
            return $"{BaseUrl}/channels/{_channelUserId}/videos?broadcast_type=archive&limit={limit}&offset={offset}";
        }

        private static string GetVideoMarkersEndpoint(string videoId)
        {
            return $"{BaseUrl}/videos/{videoId}/markers?api_version=5";
        }

        public KrakenTwitchClient(string channelName, Settings settings)
        {
            _channelName = channelName;
            _settings = settings;

            if (string.IsNullOrEmpty(_channelUserId))
                SetUserId();

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    MaxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private void SetUserId()
        {
            if (!string.IsNullOrWhiteSpace(_channelUserId))
                return;

            var apiEndpoint = $"https://api.twitch.tv/kraken/users?login={_channelName}&api_version=5";

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
            request.Accept = $@"application/vnd.twitchtv.{apiVersion}+json";

            // Need to specify the client ID https://blog.twitch.tv/client-id-required-for-kraken-api-calls-afbb8e95f843#.j496nqkhq  
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

        public async Task<Channel> GetChannelVideosAsync()
        {
            var channel = new Channel(_channelName);

            var totalVideos = await _retryPolicy.ExecuteAsync(async () => await GetTotalVideoCountAsync());

            const int limit = 50;

            for (var offset = 0; offset < totalVideos; offset += limit)
            {
                var retrievedVideos = await _retryPolicy.ExecuteAsync(async () => await GetVideosAsync(limit, offset));

                foreach (var video in retrievedVideos)
                {
                    await _retryPolicy.ExecuteAsync(async () =>
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
                dynamic jsonData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(readerOutput));

                foreach (var data in jsonData.videos)
                {
                    var video = Video.FromJson(data);
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

            dynamic jsonData;

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var readerOutput = reader.ReadToEnd();
                jsonData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(readerOutput));
            }

            if (jsonData.markers.game_changes == null)
                return;

            foreach (var data in jsonData.markers.game_changes)
            {
                video.AddMarker(Marker.FromJson(data));
            }
        }
    }
}
