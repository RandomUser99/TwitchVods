using System;
using System.Collections.Generic;
using System.Linq;
using Twitch.Net.Models;

namespace TwitchVods.Core.Models
{
    using Twitch;

    public class Video
    {
        private readonly IList<Marker> _markers = new List<Marker>();

        public string Id { get; private set; }
        public string Title { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string Game { get; private set; }
        public int Length { get; private set; }
        public string Url { get; private set; }
        public int Views { get; private set; }
        public string Duration { get; private set; }

        public IEnumerable<Marker> Markers => _markers.OrderBy(x => x.TimeSeconds).ToList();

        private Video() { }

        public static Video FromVideoData(HelixVideo data)
        {
            return new Video
            {
                Id = data.Id,
                Title = data.Title,
                CreatedAt = data.CreatedAt,
                Url = data.Url,
                Duration = data.Duration,
                Views = data.ViewCount,
                Length = DurationParser.ParseToLenthInMinutes(data.Duration)
            };
        }

        public void AddMarker(Marker marker)
        {
            if (!_markers.Any())
            {
                _markers.Add(marker);
                return;
            }

            var lastMarkerWithSameLabel = _markers.LastOrDefault(x => x.Label == marker.Label);

            if (lastMarkerWithSameLabel == null)
            {
                _markers.Add(marker);
                return;
            }

            if (OverFiveMinsSincePrevious(marker.TimeSeconds, lastMarkerWithSameLabel.TimeSeconds))
            {
                _markers.Add(marker);
                return;
            }
        }

        private static bool OverFiveMinsSincePrevious(int first, int second)
        {
            var secondsRemaining = second - first;
            var overFiveMins = secondsRemaining > 300;
            return overFiveMins;
        }
    }
}
