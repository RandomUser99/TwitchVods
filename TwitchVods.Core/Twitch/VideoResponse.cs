using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("viewable")]
        public string Viewable { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }
    }

    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }

    
}
