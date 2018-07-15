using System;

namespace TwitchVods.Core.Models
{
    internal class Marker
    {
        public string Label { get; }
        public int TimeSeconds { get; }

        public TimeSpan Timespan => TimeSpan.FromSeconds(TimeSeconds);
        public string TimeQueryString => $"{Timespan.Hours}h{Timespan.Minutes}m{Timespan.Seconds}s";

        public Marker(string label, int timeSeconds)
        {
            Label = label;
            TimeSeconds = timeSeconds;
        }

        public static Marker FromJson(dynamic jsonData)
        {
            return new Marker(jsonData.label.ToString(), int.Parse(jsonData.time.ToString()));
        }
    }
}
