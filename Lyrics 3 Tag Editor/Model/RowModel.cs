using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Lyrics_3_Tag_Editor.Utils;

namespace Lyrics_3_Tag_Editor
{
    public class RowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string time { get; set; }
        public string lyrics { get; set; }
        public SolidColorBrush status { get; set; }

        public RowModel(LyricsModel lyrics)
        {
            this.time = lyrics.getTime().getText();
            this.lyrics = lyrics.getLyrics();
            this.setStatus();
        }

        public void setTime(TimeModel time)
        {
            this.time = time.getText();
            this.setStatus();
            NotifyPropertyChanged("time");
        }

        public void setLyrics(String lyrics)
        {
            this.lyrics = lyrics;
            this.setStatus();
            NotifyPropertyChanged("lyrics");
        }

        public void setStatus()
        {
            this.status = (SolidColorBrush) new BrushConverter().ConvertFromString(LyricsUtils.Validate(time, lyrics) ? "#0F9D58" : "#DB4437");
            NotifyPropertyChanged("status");
        }
    }
}
