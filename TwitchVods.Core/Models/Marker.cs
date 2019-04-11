using System;

namespace TwitchVods.Core.Models
{
    public class Marker
    {
        public string Label { get; private set; }
        public int TimeSeconds { get; private set; }

        public TimeSpan Timespan => TimeSpan.FromSeconds(TimeSeconds);
        public string TimeQueryString => $"{Timespan.Hours}h{Timespan.Minutes}m{Timespan.Seconds}s";

        private Marker() { }

        public static Marker Create(string label, int time)
        {
            return new Marker { Label = label, TimeSeconds = time };
        }
    }
}
