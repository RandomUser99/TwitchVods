using Newtonsoft.Json;
using System.IO;

namespace TwitchVods.Core
{
    internal class Settings
    {
        public string OutputDir { get; set; }
        public string ChannelsFilePath { get; set; }
        /// <summary>
        /// Used for testing purposes. 
        /// If it is set to true, it will only pull 50 videos for each channel specified in the channels.txt file.
        /// </summary>
        public bool LimitVideos { get; set; }
        public string GoogleAnalyticsTrackingId { get; set; }
        public string TwitchApiClientId { get; set; }
        public string RedditUsername { get; set; }
        public string TwitterHandle { get; set; }

        internal static Settings FromFile()
        {
            var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            using (var reader = new StreamReader(Path.Combine(currentDir, "settings.json")))
            {
                return JsonConvert.DeserializeObject<Settings>(reader.ReadToEnd());
            }
        }
    }
}
