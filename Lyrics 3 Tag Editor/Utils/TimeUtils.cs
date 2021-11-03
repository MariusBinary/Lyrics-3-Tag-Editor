using System;
using System.Text.RegularExpressions;

namespace Lyrics_3_Tag_Editor.Utils
{
    public static class TimeUtils
    {
        public static bool Validate(string value)
        {
            return Regex.Match(value, @"^\d\d:\d\d$").Success == true ? true : false;
        }

        public class Convert
        {
            public static string ToText(TimeSpan time)
            {
                return String.Format("{0:00}:{1:00}", (int)time.Minutes, (int)time.Seconds);
            }

            public static TimeSpan ToTime(string text)
            {
                return TimeSpan.ParseExact(text, "mm':'ss", null);
            }

            public static TimeSpan ToTime(TimeSpan time)
            {
                return TimeSpan.FromSeconds(time.TotalSeconds);
            }
        }
    }
}
