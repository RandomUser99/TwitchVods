using System;
using System.Threading.Tasks;

namespace TwitchVods.Core
{
    using Twitch;

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

            var processor = new ChannelProcessor(settings);
            await processor.ProcessWorkload(channels, settings);
        }

        
    }
}

