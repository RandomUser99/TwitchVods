using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitchVods.Core.Output;
using TwitchVods.Core.Twitch;

namespace TwitchVods.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TwitchTool, starting ....");

            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var settings = GetSettings();

            var tasks = new List<Task>();

            var channels = await GetChannelsFromFile(settings);

            Console.WriteLine($"{channels.Length} channel(s) found in channels.txt file.");
            Console.WriteLine();

            foreach (var channelName in channels)
            {
                try
                {
                    tasks.Add(Task.Run(() => WriteChannelVideos(channelName.ToUpper(), settings)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await Task.WhenAll(tasks);
        }

        private static async Task<string[]> GetChannelsFromFile(Settings settings)
        {
            string fileContent;
            using (var reader = new StreamReader(settings.ChannelsFilePath))
            {
                fileContent = await reader.ReadToEndAsync();
            }

            return fileContent.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .ToList()
                            .Where(x => !x.StartsWith("//")).ToArray();
        }

        private static Settings GetSettings()
        {
            var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            using (var reader = new StreamReader(Path.Combine(currentDir, "settings.json")))
            {
                return JsonConvert.DeserializeObject<Settings>(reader.ReadToEnd());
            }
        }

        private static async Task WriteChannelVideos(string channelName, Settings settings)
        {
            var client = new KrakenTwitchClient(channelName, settings);

            var channel = await client.GetChannelVideosAsync();

            var archiveWriter = new WebPageOutput(channel, settings);
            await archiveWriter.WriteOutputAsync();

            var jsonWriter = new JsonFileOutput(channel, settings);
            jsonWriter.WriteOutput();
        }
    }
}

