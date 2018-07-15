using System;

namespace TwitchVods.Core.Models
{
    internal class Marker
    {
        public string Label { get; }
        public int TimeSeconds { get; }

        public TimeSpan Timespan => TimeSpan.FromSeconds(TimeSeconds);
        public string TimeQueryString => $"{Timespan.Hours}h{Timespan.Minutes}m{Timespan.Seconds}s";

        private Marker() { }

        public Marker(string label, int timeSeconds)
        {
            Label = label;
            TimeSeconds = timeSeconds;
        }

        public static Marker Create(string label, int time)
        {
            return new Marker(label, time);
        }
    }
}
