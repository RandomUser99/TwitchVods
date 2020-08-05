using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchVods.Core.Models
{
    public class Channel
    {
        public string Name { get; private set; }
        private readonly IList<string> _games = new List<string>();
        private readonly IList<Video> _videos = new List<Video>();
        public DateTime DateGenerated => DateTime.Now;

        public IEnumerable<Video> Videos => _videos;
        public IEnumerable<string> Games => _games.OrderBy(x => x);

        private Channel() { }

        public static Channel Create(string name)
        {
            return new Channel { Name = name };
        }

        public void AddVideoRange(IEnumerable<Video> videos)
        {
            foreach (var video in videos)
                AddVideo(video);
        }

        public void AddVideo(Video video)
        {
            if (video == null)
                return;

            _videos.Add(video);

            AddGame(video.Game);

            foreach (var marker in video.Markers)
                AddGame(marker.Label);
        }

        private void AddGame(string game)
        {
            if (string.IsNullOrEmpty(game))
                return;

            var gameName = game.Trim();

            if (!string.IsNullOrWhiteSpace(gameName) && !_games.Contains(gameName))
                _games.Add(gameName);
        }

        public int TotalVideoCount => _videos.Count;

        public int TotalViewCount => _videos.Sum(x => x.Views);

        public int TotalGamesCount => _games.Count;

        public int TotalBroadcastHours
        {
            get
            {
                var timeSpan = TimeSpan.FromSeconds(_videos.Sum(x => x.Length));

                return Convert.ToInt32(timeSpan.TotalHours);
            }
        }

        public int TotalBroadcastDays
        {
            get
            {
                var timeSpan = TimeSpan.FromSeconds(_videos.Sum(x => x.Length));

                return Convert.ToInt32(timeSpan.TotalHours) / 24;
            }
        }

        public string AverageBroadcastTime
        {
            get
            {
                var averageLength = (_videos.Sum(x => x.Length) / TotalVideoCount);
                var timeSpan = TimeSpan.FromSeconds(averageLength);

                return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
        }
    }
}
