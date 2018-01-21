namespace TwitchVods.Core
{
    public class Settings
    {
        public string OutputDir { get; set; }
        public string ChannelsFilePath { get; set; }
        public bool LimitVideos { get; set; }
        public string GoogleAnalyticsTrackingId { get; set; }
        public string TwitchApiClientId { get; set; }
    }
}
