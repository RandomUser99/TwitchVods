using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TwitchVods.Core.Models
{
    public class Video
    {
        private readonly IList<Marker> _markers = new List<Marker>();

        public string Id { get; }
        public string Title { get; }
        public long? BroadcastId { get; }
        public DateTime CreatedAt { get; }
        public string Game { get; }
        public int Length { get; }
        public string Url { get; }
        public int Views { get; }

        public IEnumerable<Marker> Markers => _markers.OrderBy(x => x.TimeSeconds).ToList();

        public Video(string id, string title, dynamic broadcastId, DateTime createdAt, string game, int length, string url, int views)
        {
            Id = id;
            Title = title;
            BroadcastId = broadcastId;
            CreatedAt = createdAt;
            Game = game;
            Length = length;
            Url = url;
            Views = views;
        }

        public string RunTime
        {
            get
            {
                var t = TimeSpan.FromSeconds(Length);
                return $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
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

        public static Video FromJson(dynamic data)
        {
            string tempId = data._id;
            var id = Regex.Replace(tempId, "[^0-9]+", string.Empty);

            return new Video(
                id,
                data.title.ToString(),
                long.Parse(data.broadcast_id.ToString()),
                DateTime.ParseExact(data.created_at.ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture),
                data.game.ToString() ?? "",
                int.Parse(data.length.ToString()),
                data.url.ToString(),
                int.Parse(data.views.ToString())
                );
        }
    }
}
