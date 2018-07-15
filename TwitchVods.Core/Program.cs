using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TwitchVods.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TwitchTool, starting");

            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var settings = GetSettings();

            var tasks = new List<Task>();

            foreach (var channelName in await GetChannelsFromFile(settings))
            {
                tasks.Add(Task.Run(() => new Worker(settings)
                    .WriteChannelVideos(channelName.ToUpper())));
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
    }
}

