using TwitchVods.Core.Models;

namespace TwitchVods.Core.Templates
{
    public class ChannelModel
    {
        public Channel Channel { get; set; }
        public string GoogleAnalyticsTrackingId { get; set; }
        public string RedditUsername { get; set; }
    }
}
