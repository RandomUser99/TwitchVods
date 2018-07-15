using System;
using System.Collections.Generic;

namespace TwitchVods.Core.Twitch.Kraken
{
    internal class KrakenVideoResponse
    {
        public int _total { get; set; }
        public List<KrakenVideo> videos { get; set; }

        public class KrakenVideo
        {
            public string title { get; set; }
            public object description { get; set; }
            public object description_html { get; set; }
            public object broadcast_id { get; set; }
            public string broadcast_type { get; set; }
            public string status { get; set; }
            public string tag_list { get; set; }
            public int views { get; set; }
            public string url { get; set; }
            public string language { get; set; }
            public DateTime created_at { get; set; }
            public string viewable { get; set; }
            public object viewable_at { get; set; }
            public DateTime published_at { get; set; }
            public string _id { get; set; }
            public DateTime recorded_at { get; set; }
            public string game { get; set; }
            public List<object> communities { get; set; }
            public int length { get; set; }
            public string restriction { get; set; }
        }
    }
}
