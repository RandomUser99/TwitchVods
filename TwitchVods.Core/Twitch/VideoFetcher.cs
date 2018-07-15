using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TwitchVods.Core.Models;

namespace TwitchVods.Core.Twitch
{
    // https://github.com/justintv/twitch-api
    public class VideoFetcher : TwitchBase
    {
        public VideoFetcher(string twitchApiClientId) : base(twitchApiClientId) { }

        public async Task<int> GetTotalVideoCount(string channelName)
        {
            if (string.IsNullOrEmpty(ChannelUserId))
                SetUserId(channelName);

            // Only need one vod to get the total available
            const int limit = 1;
            const int offset = 0;

            var apiEndpoint = GetChannelVideosEndpoint(limit, offset);
            var request = CreateWebRequest(apiEndpoint, TwitchApiVersion.v5);
            var webResponse = await request.GetResponseAsync();
            var count = 0;

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var readerOutput = await reader.ReadToEndAsync();
                dynamic jsonData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(readerOutput));

                count = jsonData._total;
            }
            return count;
        }

        public async Task<List<Video>> GetVideos(string channelName, int limit = 100, int offset = 0)
        {
            if (string.IsNullOrEmpty(ChannelUserId))
                SetUserId(channelName);

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

        private string GetChannelVideosEndpoint(int limit, int offset)
        {
            return $"https://api.twitch.tv/kraken/channels/{ChannelUserId}/videos?broadcast_type=archive&limit={limit}&offset={offset}";
        }
    }
}
