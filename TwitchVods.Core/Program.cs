﻿using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMarkupMin.Core;

namespace TwitchVods.Core
{
    using Models;
    using Output;
    using PageGeneration;
    using Twitch.Kraken;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TwitchTool, starting ....");

            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var settings = Settings.FromFile();
            var channels = await Channels.FromFileAsync(settings.ChannelsFilePath);

            Console.WriteLine($"{channels.Length} channel(s) found in channels.txt file:");

            foreach (var channel in channels)
            {
                Console.WriteLine(channel);
            }

            Console.WriteLine();
            Console.WriteLine("Fetching archive videos ....");

            await ProcessWorkload(channels, settings);
        }

        private static async Task ProcessWorkload(string[] channels, Settings settings)
        {
            var retryPolicy = Policies.RetryPolicy();
            var pageCreator = new PageCreator(new HtmlGenerator(), new HtmlMinifier());
            var tasks = new List<Task>();

            foreach (var channelName in channels)
            {
                try
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var channel = await ChannelVideos(channelName.ToUpper(), settings, retryPolicy);
                        if (channel.TotalVideoCount > 0)
                        {
                            await pageCreator.CreateChannelPageAsync(channel, settings);
                            new JsonFileOutput(channel, settings).WriteOutput();
                        }
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

        private static async Task<Channel> ChannelVideos(string channelName, Settings settings, IAsyncPolicy retryPolicy)
        {
            var client = new KrakenTwitchClient(channelName, settings, retryPolicy);

            return await client.ChannelVideosAsync();
        }
    }
}

