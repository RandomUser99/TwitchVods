using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twitch.Net;
using Twitch.Net.Models;
using TwitchVods.Core.Models;

namespace TwitchVods.Core.Twitch
{
    public class VideoFetcher
    {
        private readonly TwitchApi _api;

        public VideoFetcher(TwitchApi api)
        {
            _api = api;
        }

        public async Task<Channel> ChannelVideosAsync(HelixUser channelUser)
        {
            var channel = Channel.Create(channelUser.DisplayName);

            var retrievedVideos = await GetVideosAsync(channelUser);

            // Markers cannot be retrieved unless you created them:
            // https://dev.twitch.tv/docs/api/reference#get-stream-markers
            // This has been raised as a request to be added to the Helix API.

            //foreach (var video in retrievedVideos)
            //{
            //    await _retryRetryPolicy.ExecuteAsync(async () =>
            //    {
            //        await PopulateMarkersAsync(video);
            //    });
            //}

            channel.AddVideoRange(retrievedVideos);

            return channel;
        }

        private async Task<List<Video>> GetVideosAsync(HelixUser channelUser)
        {
            var videos = new List<Video>();
            var cursor = string.Empty;

            do
            {
                var response = await _api.GetVideosFromUser(channelUser.Id, after: cursor, type: "archive", first: 100);
                var videosData = response.Data.Select(Video.FromVideoData);
                videos.AddRange(videosData);
                cursor = response.Pagination.Cursor;

                Console.WriteLine("{0} Retrieved: {1}", channelUser.DisplayName, videos.Count);

            } while (!string.IsNullOrEmpty(cursor));

            return videos;
        }
    }
}
