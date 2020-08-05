using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Net;
using Twitch.Net.Models;

namespace TwitchVods.Core.Twitch
{
    using Models;

    public class VideoFetcher
    {
        private readonly TwitchApi _client;
        private readonly bool _limitVideos;

        public VideoFetcher(TwitchApi client, bool limitVideos)
        {
            _client = client;
            _limitVideos = limitVideos;
        }

        public async Task<Channel> ChannelVideosAsync(HelixUser channelUser)
        {
            var channel = Channel.Create(channelUser.DisplayName);

            var retrievedVideos = await GetVideosAsync(channelUser);

            // need to populate markers when twitch sort out getting markers for a video ...

            channel.AddVideoRange(retrievedVideos);

            return channel;
        }

        private async Task<List<Video>> GetVideosAsync(HelixUser channelUser)
        {
            var videos = new List<Video>();
            var cursor = string.Empty;

            do
            {
                var response = await _client.GetVideosFromUser(channelUser.Id, after: cursor, type: "archive", first: 100);
                var videosData = response.Data.Select(Video.FromVideoData);
                videos.AddRange(videosData);
                cursor = response.Pagination.Cursor;

                Console.WriteLine("{0} Retrieved: {1}", channelUser.DisplayName, videos.Count);

                if (_limitVideos)
                {
                    break;
                }

            } while (!string.IsNullOrEmpty(cursor));

            return videos;
        }
    }
}
