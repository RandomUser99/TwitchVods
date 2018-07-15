using Newtonsoft.Json;
using System.IO;
using TwitchVods.Core.Models;

namespace TwitchVods.Core.Output
{
    public class JsonFileOutput 
    {
        private readonly Channel _channel;
        private readonly Settings _settings;
        
        public JsonFileOutput(Channel channel, Settings settings)
        {
            _channel = channel;
            _settings = settings;
            GetFilePath();
        }

        private string GetFilePath()
        {
            var outputDir = _settings.OutputDir;
            var outputFileName = $"{_channel.Name.ToLower()}.json";
            var outputFilePath = Path.Combine(outputDir, outputFileName);
            return outputFilePath;
        }

        public void WriteOutput()
        {
            // serialize JSON directly to a file
            using (var file = File.CreateText(GetFilePath()))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, _channel);
            }
        }
    }
}
