using Shouldly;
using System;
using Twitch.Net.Models;
using TwitchVods.Core.Models;
using Xunit;

namespace TwitchVods.UnitTests.Models
{
    using static Randomiser;

    public class VideoShould
    {
        [Fact]
        public void create_from_video_data()
        {
            var videoData = new HelixVideo
            {
                CreatedAt = DateTime.Now,
                Duration = "1h2m3s",
                ViewCount = 100,
                Title = RandomString(12),
                Url = RandomString(15),
                Id = RandomString(5)
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
