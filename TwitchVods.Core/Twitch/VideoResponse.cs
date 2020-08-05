using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TwitchVods.Core.Twitch
{
    public class VideoResponse
    {
        [JsonProperty("data")]
        public IList<VideoData> Videos { get; set; }

        [JsonProperty("Pagination")]
        public Pagination Pagination { get; set; }
    }

    public class VideoData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("viewable")]
        public string Viewable { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }
    }

    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
}
