using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchVods.Core.Models
{
    public class Video
    {
        private readonly IList<Marker> _markers = new List<Marker>();

        public string Id { get; private set; }
        public string Title { get; private set; }
        public long? BroadcastId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string Game { get; private set; }
        public int Length { get; private set; }
        public string Url { get; private set; }
        public int Views { get; private set; }

        public IEnumerable<Marker> Markers => _markers.OrderBy(x => x.TimeSeconds).ToList();

        private Video() { }

        public static Video Create(string id, string title, long? broadcastId, DateTime createdAt, string game, int length, string url, int views)
        {
            return new Video
            {
                Id =  id, 
                Title = title,
                BroadcastId = broadcastId,
                CreatedAt = createdAt,
                Game = game,
                Length = length,
                Url = url, 
                Views = views
            };
        }

        public string RunTime
        {
            get
            {
                var timeSpan = TimeSpan.FromSeconds(Length);
                return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }

        public int RuntimeMins => TimeSpan.FromSeconds(Length).Minutes;

        public static int LengthFromDateTimeString(string runTime)
        {
            var length = TimeSpan.Parse(runTime).TotalSeconds;
            var lengthInt = int.Parse(length.ToString());
            return lengthInt;
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
