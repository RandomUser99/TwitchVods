using System.Collections.Generic;

namespace TwitchVods.Core.Twitch.Kraken
{
    internal class KrakenMarkerResponse
    {
        public string vod_id { get; set; }
        public Markers markers { get; set; }

        public class GameChange
        {
            public int time { get; set; }
            public string label { get; set; }
        }

        public class Markers
        {
            public List<GameChange> game_changes { get; set; }
        }
    }
}
