namespace Lyrics_3_Tag_Editor.Utils
{
    public static class ConversionUtils
    {
        public static string GetFrequency(int value)
        {
            return (value / 1000) + " " + "KHz";
        }

        public static string GetBitrate(int value)
        {
            return value + " " + "Kb/s";
        }
    }
}
