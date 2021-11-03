using System.Windows;

namespace Lyrics_3_Tag_Editor.Utils
{
    public static class CultureUtils
    {
        public static string Get(string value)
        {
            return Application.Current.FindResource(value) as string;
        }

        public class Language
        {
            public static string Get()
            {
                return Properties.Settings.Default.Culture;
            }

            public static void Set(string culture)
            {
                MessageBox.Show(CultureUtils.Get("msg_language_change"), CultureUtils.Get("msg_type_info"), MessageBoxButton.OK, MessageBoxImage.Information);
                Properties.Settings.Default.Culture = culture;
                Properties.Settings.Default.Save();
            }
        }
    }
}
