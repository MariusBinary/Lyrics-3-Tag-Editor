using System;

namespace Lyrics_3_Tag_Editor
{
    public class LyricsModel
    {
        public TimeModel time { get; set; } = new TimeModel();
        public String lyrics { get; set; } = null;

        public LyricsModel()
        {
        }

        public LyricsModel(String lyrics)
        {
            this.getTime().setEmpty(true);
            this.setLyrics(lyrics);
        }

        public LyricsModel(TimeSpan time, String lyrics)
        {
            this.setTime(time);
            this.setLyrics(lyrics);
        }

        public LyricsModel(String time, String lyrics)
        {
            this.getTime().setText(time);
            this.setLyrics(lyrics);
        }

        public TimeModel getTime()
        {
            return time;
        }

        public String getLyrics()
        {
            return lyrics;
        }

        public void setTime(TimeSpan value)
        {
            if (value != null)
            {
                time.setTime(value);
                return;
            }
        }

        public void setLyrics(String value)
        {
            if (value != null)
            {
                if (value != "")
                {
                    lyrics = value;
                    return;
                }
            }
        }

        public void Clear()
        {
            time.setEmpty(true);
            lyrics = null;
        }

        public bool Equals(TimeModel time)
        {
            return this.time == time;
        }
    }
}
