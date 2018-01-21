using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TwitchVods.Core.Models;
using TwitchVods.Core.Output;
using TwitchVods.Core.Twitch;

namespace TwitchVods.Core
{
    public class Worker
    {
        private readonly Settings _settings;
        private Channel _channel;
        private readonly MarkerFetcher _markerFetcher;

        public Worker(Settings settings)
        {
            _settings = settings;
            _markerFetcher = new MarkerFetcher(_settings.TwitchApiClientId);
        }

        public async Task WriteChannelVideos(string channelName)
        {
            _channel = new Channel(channelName);

            await GetVideos();

            var archiveWriter = new WebPageOutput(_channel, _settings);
            await archiveWriter.WriteOutputAsync();

            var jsonWriter = new JsonFileOutput(_channel, _settings);
            jsonWriter.WriteOutput();
        }

        private async Task GetVideos()
        {
            var videoFetcher = new VideoFetcher(_settings.TwitchApiClientId);
            int totalVideos;

            try
            {
                totalVideos = await videoFetcher.GetTotalVideoCount(_channel.Name);
            }
            catch (WebException exception)
            {
                Console.Write("\rError: {0}", exception.Message);
                return;
            }

            const int limit = 50;

            for (var offset = 0; offset < totalVideos; offset += limit)
            {
                List<Video> retrievedVideos;
                try
                {
                    retrievedVideos = await videoFetcher.GetVideos(_channel.Name, limit, offset);

                    foreach (var video in retrievedVideos)
                    {
                        await GetMarkers(video);
                    }

                }
                catch (WebException exception)
                {
                    Console.Write("\rError: {0}", exception.Message);
                    return;
                }

                if (retrievedVideos.Count > limit)
                    throw new ApplicationException(
                        "Twitch API is returning bad data. It is returning more records than the limit value.");

                _channel.AddVideoRange(retrievedVideos);

                Console.WriteLine("{0} Retreived: {1} of {2} ", _channel.Name, _channel.TotalVideoCount, totalVideos);

                // If LimitVideos is true, it will bail out. Used for testing purposes.
                if (_settings.LimitVideos)
                    break;
            }
        }

        private async Task GetMarkers(Video video)
        {
            try
            {
                await _markerFetcher.PopulateMarkers(video);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
