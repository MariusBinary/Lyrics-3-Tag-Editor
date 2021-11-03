using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lyrics_3_Tag_Editor.Utils
{
    public static class LyricsUtils
    {
        public static List<LyricsModel> ToArray(String lyrics, bool allowEmptyLines, bool allowTime)
        {
            String[] splitted_array = lyrics.Split(new[] { "\r\n", "\r", "\n" }, 
                allowEmptyLines == true ?
                StringSplitOptions.None : 
                StringSplitOptions.RemoveEmptyEntries);

            List<LyricsModel> lyrics_array = new List<LyricsModel>();

            //check if lyrics is not null
            if (lyrics != null)
            {
                //check if lyrics is not empty
                if (lyrics != "")
                {
                    //check if splitted lyrics lenght is major than 0
                    if (splitted_array.Length > 0)
                    {
                        for (int i = 0; i < splitted_array.Length; i++)
                        {
                            String line = splitted_array[i];
                            String time = null;
                            String text = null;

                            //check if line is not null
                            if (line != null)
                            {
                                var pattern = @"^?[\d\d:\d\d\?]";

                                //check if preferences allows time import
                                if (allowTime == true)
                                {
                                    //check if line contains a time part
                                    if (Regex.Match(line, pattern).Success)
                                    {
                                        time = TimeUtils.Validate(StringUtils.Between(line, "[", "]")) == true ? StringUtils.Between(line, "[", "]") : null;
                                        text = line.Replace("[" + time + "]", "");
                                    }
                                    else
                                    {
                                        text = line;
                                    }
                                }
                                else
                                {
                                    text = line;
                                }

                                //check if time is not null
                                if (time != null)
                                {
                                    lyrics_array.Add(new LyricsModel(TimeUtils.Convert.ToTime(time), text));
                                }
                                else
                                {
                                    lyrics_array.Add(new LyricsModel(text));
                                }
                            }
                            else
                            {
                                lyrics_array.Add(new LyricsModel());
                            }
                        }
                    }
                }
            }

            return lyrics_array;
        }

        public static String ToString(List<LyricsModel> lyrics, bool allowTime)
        {
            String lyrics_string = "";

            //check if lyrics is not null
            if (lyrics != null)
            {
                //check if lyrics count is major than 0
                if (lyrics.Count > 0)
                {
                    for (int i = 0; i < lyrics.Count; i++)
                    {
                        LyricsModel line = lyrics[i];
                        String splitter = (i == (lyrics.Count - 1)) ? "" : "\r\n";

                        //check if time is not empty
                        if (line.getTime().isEmpty() == false)
                        {
                            //check if lyrics is not null
                            if (line.getLyrics() != null)
                            {
                                //check if lyrics is not empty
                                if (line.getLyrics() != "")
                                {
                                    lyrics_string += (allowTime == true ? ("[" + line.getTime().getText() + "]") : "") + line.getLyrics() + splitter;
                                    continue;
                                }
                            }

                            //check if empty lyrics is allowed
                            if (Properties.Settings.Default.AllowEmptyRows == true)
                            {
                                lyrics_string += (allowTime == true ? ("[" + line.getTime().getText() + "]") : "") + splitter;
                                continue;
                            }
                        }
                    }
                }
            }

            return lyrics_string;
        }

        public static bool Validate(LyricsModel line)
        {
            //check if lyrics is not null
            if (line != null)
            {
                //check if time is not empty
                if (line.getTime().isEmpty() == false)
                {
                    //check if lyrics is not null
                    if (line.getLyrics() != null)
                    {
                        //check if lyrics is not empty
                        if (line.getLyrics() != "")
                        {
                            return true;
                        }
                    }

                    //check if empty lyrics is allowed
                    if (Properties.Settings.Default.AllowEmptyRows == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool Validate(string time, string lyrics)
        {
            //check if time is not null
            if (time != null)
            {
                //check if time is not empty
                if (time != "")
                {
                    //check if lyrics is not null
                    if (lyrics != null)
                    {
                        //check if lyrics is not empty
                        if (lyrics != "")
                        {
                            return true;
                        }
                    }

                    //check if empty lyrics is allowed
                    if (Properties.Settings.Default.AllowEmptyRows == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<LyricsModel> Sort(List<LyricsModel> lyrics, TimeSpan time)
        {
            lyrics.Sort(delegate (LyricsModel c1, LyricsModel c2) {
                TimeSpan t1 = c1.getTime().isEmpty() == true ? time : c1.getTime().getSpan();
                TimeSpan t2 = c2.getTime().isEmpty() == true ? time : c2.getTime().getSpan();
                return t1.CompareTo(t2);
            });

            return lyrics;
        }
    }
}
