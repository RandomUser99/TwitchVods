using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitch.Net;
using Twitch.Net.Models;

namespace TwitchVods.Core.Twitch
{
    using Models;
    using Output;
    using PageGeneration;

    internal class ChannelProcessor
    {
        private readonly Settings _settings;
        private readonly TwitchApi _twitchClient;

        public ChannelProcessor(Settings settings)
        {
            _settings = settings;
            _twitchClient = new TwitchApiBuilder(settings.TwitchApiClientId)
                .WithClientSecret(settings.TwitchApiClientSecret)
                .WithRateLimitBypass()
                .Build();
        }

        public async Task ProcessWorkload(string[] channels, Settings settings)
        {
            var pageCreator = new PageCreator(new HtmlGenerator(), new HtmlMinifierAdapter());
            var tasks = new List<Task>();

            var channelUsers = await _twitchClient.GetUsersWithLoginNames(channels);

            foreach (var channelUser in channelUsers.Data)
            {
                try
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var channel = await ChannelVideos(channelUser);
                        await pageCreator.CreateChannelPageAsync(channel, settings);
                        new JsonFileOutput(channel, settings).WriteOutput();
                    }));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await pageCreator.CreateIndexPageAsync(channels, settings.OutputDir);

            await Task.WhenAll(tasks);
        }

        private async Task<Channel> ChannelVideos(HelixUser channelUser)
        {
            var videoFetcher = new VideoFetcher(_twitchClient, _settings.LimitVideos);
            return await videoFetcher.ChannelVideosAsync(channelUser);
        }
    }
}
