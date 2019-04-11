using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitchVods.Core.Output;
using TwitchVods.Core.Templates;
using TwitchVods.Core.Twitch.Kraken;

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

            Console.WriteLine($"{channels.Length} channel(s) found in channels.txt file:");
            foreach (var channel in channels)
            {
                Console.WriteLine(channel);
            }

            Console.WriteLine();
            Console.WriteLine("Fetching archive videos ....");

            var retryPolicy = GetRetryPolicy();

            foreach (var channelName in channels)
            {
                try
                {
                    tasks.Add(Task.Run(() => WriteChannelVideos(channelName.ToUpper(), settings, retryPolicy)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await WriteIndexPageAsync(channels, settings.OutputDir);

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

        private static async Task WriteChannelVideos(string channelName, Settings settings, IAsyncPolicy retryPolicy)
        {
            var client = new KrakenTwitchClient(channelName, settings, retryPolicy);

            var channel = await client.GetChannelVideosAsync();

            //await new WebPageOutput(channel, settings).WriteOutputAsync();
            var model = new ChannelModel
            {
                Channel = channel,
                GoogleAnalyticsTrackingId = settings.GoogleAnalyticsTrackingId,
                TwitterHandle = settings.TwitterHandle,
            };

            var markup = await new ChannelGenerator().GenerateMarkupAsync(model);

            using (var outputFile = new StreamWriter($"{settings.OutputDir}/{channelName.ToLower()}.html"))
            {
                await outputFile.WriteAsync(markup);
                await outputFile.FlushAsync();
            }

            new JsonFileOutput(channel, settings).WriteOutput();
        }

        private static async Task WriteIndexPageAsync(string[] channels, string outputPath)
        {
            var htmlGeneratory = new IndexGenerator();
            var markup = await htmlGeneratory.GenerateMarkupAsync(new IndexModel { Channels = channels });

            using (var indexFile = new StreamWriter($"{outputPath}/index.html"))
            {
                await indexFile.WriteAsync(markup);
                await indexFile.FlushAsync();
            }
        }

        private static IAsyncPolicy GetRetryPolicy()
        {
            const int maxRetries = 10;

            return Policy.Handle<Exception>()
                .WaitAndRetryAsync(
                    maxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}

