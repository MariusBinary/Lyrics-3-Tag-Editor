using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lyrics_3_Tag_Editor
{
    public class FileModel
    {
        public FileInfo Original { get; set; }
        public FileInfo Temporary { get; set; }

        public String Name { get; set; }
        public String Bitrate { get; set; }
        public String Frequency { get; set; }
        public TimeSpan Duration { get; set; }
        public Tags Tag { get; set; }

        public class Tags
        {
            public String Title { get; set; }
            public String Artist { get; set; }
            public String Album { get; set; }
            public uint Track { get; set; }
            public uint Year { get; set; }
            public BitmapImage CovertArt { get; set; }
        }

        public FileModel(String f1, String f2)
        {
            this.Original = new FileInfo(f1);
            this.Temporary = new FileInfo(f2);

            Tag = new Tags();
            TagLib.File file = TagLib.File.Create(Original.FullName);
            this.Name = Original.FullName;
            this.Bitrate = Utils.ConversionUtils.GetBitrate(file.Properties.AudioBitrate);
            this.Frequency = Utils.ConversionUtils.GetFrequency(file.Properties.AudioSampleRate);
            this.Duration = file.Properties.Duration;

            this.Tag.Title = file.Tag.Title;
            this.Tag.Artist = file.Tag.FirstPerformer;
            this.Tag.Album = file.Tag.Album;
            this.Tag.Track = file.Tag.Track;
            this.Tag.Year = file.Tag.Year;

            if (file.Tag.Pictures.Length >= 1)
            {
                var bin = (byte[])(file.Tag.Pictures[0].Data.Data);
                Image img = Image.FromStream(new MemoryStream(bin));

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Jpeg);
                ms.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = ms;
                bi.EndInit();
                this.Tag.CovertArt = bi;
            }
            else
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(@"pack://application:,,,/Images/default_album_art.png", UriKind.RelativeOrAbsolute);
                bi.EndInit();
                this.Tag.CovertArt = bi;
            }

            file.Dispose();
        }
    }
}
