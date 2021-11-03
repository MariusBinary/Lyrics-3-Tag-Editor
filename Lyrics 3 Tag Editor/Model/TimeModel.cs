using System;
using Lyrics_3_Tag_Editor.Utils;

namespace Lyrics_3_Tag_Editor
{
    public class TimeModel
    {
        private TimeSpan time { get; set; } = new TimeSpan();
        private int sec { get; set; } = -1;
        private String text { get; set; } = "";
        private Boolean empty { get; set; } = true;

        public TimeModel()
        {
        }

        public TimeModel(TimeSpan value)
        {
            setTime(value);
        }

        public TimeSpan getSpan()
        {
            return time;
        }

        public int getSec()
        {
            return sec;
        }

        public String getText()
        {
            return text;
        }

        public Boolean isEmpty()
        {
            return empty;
        }

        public void setTime(TimeSpan value)
        {
            if (value != null)
            {
                this.time = value;
                this.sec = (int)value.TotalSeconds;
                this.text = TimeUtils.Convert.ToText(value);
                setEmpty(false);
            }
            else
            {
                setEmpty(true);
            }
        }

        public void setText(String text)
        {
            if (text != null)
            {
                if (text != "")
                {
                    this.time = TimeUtils.Convert.ToTime(text);
                    this.sec = (int)time.TotalSeconds;
                    this.text = text;
                    setEmpty(false);
                }
                else
                {
                    setEmpty(true);
                }
            }
            else
            {
                setEmpty(true);
            }
        }

        public void setEmpty(Boolean value)
        {
            if (value == true)
            {
                time = new TimeSpan();
                text = "";
                empty = true;
            }
            else
            {
                empty = false;
            }
        }
    }
}
