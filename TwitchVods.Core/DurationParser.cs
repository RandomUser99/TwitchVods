using System;

namespace TwitchVods.Core
{
    public static class DurationParser
    {
        public static int ParseToLenthInMinutes(string duration)
        {
            var durationWithColons = duration.Replace("h", ":").Replace("m", ":").Replace("s", string.Empty);

            var splitDuration = durationWithColons.Split(':');

            var days = 0;
            var hours = 0;
            var minutes = 0;
            var seconds = 0;

            if (splitDuration.Length == 3)
            {
                hours = int.Parse(splitDuration[0]);
                minutes = int.Parse(splitDuration[1]);
                seconds = int.Parse(splitDuration[2]);
            }

            if (splitDuration.Length == 2)
            {
                minutes = int.Parse(splitDuration[0]);
                seconds = int.Parse(splitDuration[1]);
            }

            if (splitDuration.Length == 1)
            {
                seconds = int.Parse(splitDuration[0]);
            }

            if (hours >= 48 && hours < 72)
            {
                days = 2;
                hours -= 48;
            }

            if (hours >= 24 && hours < 48)
            {
                days = 1;
                hours -= 24;
            }

            return new TimeSpan(days, hours, minutes, seconds).Minutes;
        }
    }
}
