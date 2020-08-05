using System;
using Shouldly;
using TwitchVods.Core.Models;
using TwitchVods.Core.Twitch;
using Xunit;

namespace TwitchVods.UnitTests.Models
{
    using static Randomiser;

    public class VideoShould
    {
        [Fact]
        public void calculate_correct_video_length()
        {
            var video = Video.FromVideoData(new VideoData { Duration = "7h20m12s" });

            var expected = new TimeSpan(0, 7, 20, 12).Minutes;
            video.Length.ShouldBe(expected);
        }

        [Fact]
        public void create_from_video_data()
        {
            var videoData = new VideoData
            {
                CreatedAt = DateTime.Now,
                Duration = "1h2m3s",
                ViewCount = 100,
                Title = RandomString(12),
                Url = RandomString(15),
                Id = RandomString(5),
                UserName = RandomString(10)
            };

            var test = Video.FromVideoData(videoData);

            test.CreatedAt.ShouldBe(videoData.CreatedAt);
            test.Duration.ShouldBe(videoData.Duration);
            test.Views.ShouldBe(videoData.ViewCount);
            test.Title.ShouldBe(videoData.Title);
            test.Url.ShouldBe(videoData.Url);
            test.Id.ShouldBe(videoData.Id);
        }
    }
}
