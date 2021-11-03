using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Lyrics_3_Tag_Editor.Utils;

namespace Lyrics_3_Tag_Editor.Services
{
    public class MediaManager
    {
        #region Callback

        public Callback.onMediaLoaded onMediaLoaded { get; set; }
        public Callback.onMediaCleaned onMediaCleaned { get; set; }

        public class Callback
        {
            public delegate void onMediaLoaded(String f1, String f2);
            public delegate void onMediaCleaned();
        }

        #endregion

        #region Variables

        public bool IsMediaPresent { get; set; } = false;
        private List<String> allFiles = null;

        #endregion

        public MediaManager()
        {
            allFiles = new List<string>();
        }

        public int Load(String path)
        {
            try
            {
                FileInfo file = new FileInfo(path);

                if (file.IsReadOnly == true)
                {
                    if (MessageBox.Show(CultureUtils.Get("msg_remove_readonly"), CultureUtils.Get("msg_type_confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            file.IsReadOnly = false;
                        }
                        catch
                        {
                            return 3;
                        }
                    }
                    else
                    {
                        return 2;
                    }
                }

                String tempPath = Path.GetTempPath() + @"/Lyrics 3 Tag Editor/";

                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }

                String originalFile = file.FullName;
                String tempFile = tempPath + file.Name;

                File.Copy(originalFile, tempFile, true);

                if (!allFiles.Contains(tempFile))
                {
                    allFiles.Add(tempFile);
                }

                IsMediaPresent = true;
                onMediaLoaded?.Invoke(originalFile, tempFile);
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        public void Clear()
        {
            try
            {
                if (allFiles != null && allFiles.Count != -1)
                {
                    for (int i = 0; i < allFiles.Count; i++)
                    {
                        if (File.Exists(allFiles[i]))
                        {
                            File.Delete(allFiles[i]);
                        }
                    }
                }

                onMediaCleaned?.Invoke();
            }
            catch
            {
                return;
            }
        }
    }
}
