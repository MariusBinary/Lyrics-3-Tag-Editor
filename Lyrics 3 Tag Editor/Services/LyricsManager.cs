using org.farng.mp3;
using org.farng.mp3.lyrics3;
using System;
using System.Collections.Generic;
using System.Windows;
using Lyrics_3_Tag_Editor.Utils;

namespace Lyrics_3_Tag_Editor.Services
{
    public class LyricsManager
    {
        #region Callback

        public Callback.onMediaLoaded onMediaLoaded { get; set; }
        public Callback.onMediaSaved onMediaSaved { get; set; }

        public class Callback
        {
            public delegate void onMediaLoaded(List<LyricsModel> lyrics);
            public delegate void onMediaSaved();
        }

        #endregion

        #region Variables

        private MP3File MP3File = null;

        #endregion

        private bool Load(MP3File file)
        {
            try
            {
                this.MP3File = file;
                TagOptionSingleton.getInstance().setDefaultSaveMode(TagConstant.MP3_FILE_SAVE_OVERWRITE);
                TagOptionSingleton.getInstance().setOriginalSavedAfterAdjustingID3v2Padding(Properties.Settings.Default.AllowOriginalFile ? true : false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Load(FileModel file)
        {
            try
            {
                this.Load(new MP3File(file.Original.FullName));
                List<LyricsModel> lyrics = new List<LyricsModel>();

                if (MP3File.hasLyrics3Tag())
                {
                    AbstractLyrics3 lyricsTags = MP3File.getLyrics3Tag();
                    lyrics = LyricsUtils.ToArray(lyricsTags.getSongLyric(), true, true);
                }

                onMediaLoaded?.Invoke(lyrics);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Save(List<LyricsModel> lyrics)
        {
            try
            {
                if (MP3File != null)
                {
                    Lyrics3v2 lyrics3v2 = new Lyrics3v2();
                    String str_lyrics = LyricsUtils.ToString(lyrics, true);

                    lyrics3v2.setSongLyric(str_lyrics);
                    MP3File.setLyrics3Tag(lyrics3v2);
                    MP3File.save();

                    onMediaSaved?.Invoke();
                    return true;
                }

                MessageBox.Show(CultureUtils.Get("msg_lyrics_reading_error"), CultureUtils.Get("msg_type_info"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch
            {
                return false;
            }
        }


        public void Clear()
        {
            if (MP3File != null)
            {
                MP3File = null;
            }
        }
    }
}