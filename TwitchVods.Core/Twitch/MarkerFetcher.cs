using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using TwitchVods.Core.Models;

namespace TwitchVods.Core.Twitch
{
    public class MarkerFetcher : TwitchBase
    {
        public MarkerFetcher(string twitchApiClientId) : base(twitchApiClientId) { }

        //https://discuss.dev.twitch.tv/t/vod-game-changed-times/10880

        private Video _video;

        public async Task PopulateMarkers(Video video)
        {
            _video = video;

            var apiEndpoint = GetVideoMarkersEndpoint(video.Id);

            var request = CreateWebRequest(apiEndpoint, TwitchApiVersion.v5);

            var webResponse = await request.GetResponseAsync();

            dynamic jsonData;

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var readerOutput = reader.ReadToEnd();
                jsonData = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(readerOutput));
            }

            if (jsonData.markers.game_changes == null)
                return;

            foreach (var data in jsonData.markers.game_changes)
            {
                AddMarker(data);
            }
        }

        private void AddMarker(dynamic data)
        {
            var marker = Marker.FromJson(data);

            _video.AddMarker(marker);
        }

        private static string GetVideoMarkersEndpoint(string videoId)
        {
            return $"https://api.twitch.tv/kraken/videos/{videoId}/markers?api_version=5";
        }
    }
}
