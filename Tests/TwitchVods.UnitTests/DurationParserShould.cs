using Shouldly;
using System;
using TwitchVods.Core;
using TwitchVods.Core.Models;
using TwitchVods.Core.Twitch;
using Xunit;

namespace TwitchVods.UnitTests
{
    public class DurationParserShould
    {
        [Fact]
        public void calculate_correct_video_length()
        {
            var duration = "7h20m12s";
            var test = DurationParser.ParseToLenthInMinutes(duration);

            var expected = new TimeSpan(0, 7, 20, 12).Minutes;
            test.ShouldBe(expected);
        }

        [Fact]
        public void calculate_correct_video_length_without_hours()
        {
            var duration = "43m0s";
            var test = DurationParser.ParseToLenthInMinutes(duration);

            var expected = new TimeSpan(0, 0, 43, 0).Minutes;
            test.ShouldBe(expected);
        }

        [Fact]
        public void calculate_correct_video_length_when_over_a_day()
        {
            var duration = "24:34:33";
            var test = DurationParser.ParseToLenthInMinutes(duration);

            var expected = new TimeSpan(1, 0, 34, 33).Minutes;
            test.ShouldBe(expected);
        }

        [Fact]
        public void calculate_correct_video_length_when_over_two_days()
        {
            var duration = "50:22:14";
            var test = DurationParser.ParseToLenthInMinutes(duration);

            var expected = new TimeSpan(2, 2, 22, 14).Minutes;
            test.ShouldBe(expected);
        }
    }
}
