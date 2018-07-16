using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchVods.Core.Models
{
    internal class Channel
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

        public void AddVideos(IEnumerable<Video> videos)
        {
            foreach (var video in videos)
                AddVideo(video);
        }

        public int TotalVideoCount => _videos.Count;

        public int TotalViewCount => _videos.Sum(x => x.Views);

        public int TotalGamesCount => _games.Count;

        public int TotalBroadcastHours
        {
            get
            {
                var t = TimeSpan.FromSeconds(_videos.Sum(x => x.Length));

                return Convert.ToInt32(t.TotalHours);
            }
        }

        public int TotalBroadcastDays
        {
            get
            {
                var t = TimeSpan.FromSeconds(_videos.Sum(x => x.Length));

                return Convert.ToInt32(t.TotalHours) / 24;
            }
        }

        public string AverageBroadcastTime
        {
            get
            {
                var averageLength = (_videos.Sum(x => x.Length) / TotalVideoCount);
                var t = TimeSpan.FromSeconds(averageLength);

                return $"{t.Hours}h {t.Minutes}m {t.Seconds}s";
            }
        }
    }
}
