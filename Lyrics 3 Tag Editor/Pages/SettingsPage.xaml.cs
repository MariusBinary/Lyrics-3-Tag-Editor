using System.Windows;
using System.Windows.Controls;

namespace Lyrics_3_Tag_Editor.Pages
{
    public partial class SettingsPage : Window
    {
        MainWindow context = null;

        public SettingsPage(MainWindow context)
        {
            InitializeComponent();
            this.context = context;

            rad_general.IsChecked = true;
            changeTab(Section.General);

            cb_allow_original_file.IsChecked = Properties.Settings.Default.AllowOriginalFile;
            cb_allow_auto_sort.IsChecked = Properties.Settings.Default.AllowAutoSort;
            cb_allow_empty_rows.IsChecked = Properties.Settings.Default.AllowEmptyRows;
            cb_allow_row_status.IsChecked = Properties.Settings.Default.AllowRowStatus;
            cb_allow_import_blank_lines.IsChecked = Properties.Settings.Default.AllowImportBlankLines;
            cb_allow_import_time.IsChecked = Properties.Settings.Default.AllowImportTime;
            cb_allow_export_time.IsChecked = Properties.Settings.Default.AllowExportTime;
            cb_allow_files_drag_and_drop.IsChecked = Properties.Settings.Default.AllowFilesDragAndDrop;

            switch (Properties.Settings.Default.Layout)
            {
                case 0:
                    sel_classic_layout.IsChecked = true;
                    break;
                case 1:
                    sel_invert_layout.IsChecked = true;
                    break;
                case 2:
                    sel_single_layout.IsChecked = true;
                    break;
            }
        }

        private void sel_classic_layout_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Layout = 0;
            Properties.Settings.Default.Save();
        }

        private void sel_invert_layout_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Layout = 1;
            Properties.Settings.Default.Save();
        }

        private void sel_single_layout_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Layout = 2;
            Properties.Settings.Default.Save();
        }

        private void cb_allow_auto_sort_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowAutoSort = cb_allow_auto_sort.IsChecked == true ? true : false;
            Properties.Settings.Default.Save();
        }

        private void cb_allow_save_blank_rows_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowEmptyRows = cb_allow_empty_rows.IsChecked == true ? true : false;
            Properties.Settings.Default.Save();
        }

        private void cb_allow_original_file_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowOriginalFile = cb_allow_original_file.IsChecked == true ? true : false;
            Properties.Settings.Default.Save();
        }

        private void cb_allow_row_status_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowRowStatus = cb_allow_row_status.IsChecked == true ? true : false;
            Properties.Settings.Default.Save();
        }

        private void cb_allow_import_blank_lines_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowImportBlankLines = cb_allow_import_blank_lines.IsChecked == true ? true : false;
            Properties.Settings.Default.Save();
        }

        private void cb_allow_import_time_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowImportTime = cb_allow_import_time.IsChecked == true ? true : false;
            Properties.Settings.Default.Save();
        }

        private void cb_allow_export_time_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowExportTime = cb_allow_export_time.IsChecked == true ? true : false;
            Properties.Settings.Default.Save();
        }

        private void cb_allow_files_drag_and_drop_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AllowFilesDragAndDrop = cb_allow_files_drag_and_drop.IsChecked == true ? true : false;
            Properties.Settings.Default.Save();
        }

        private void rad_general_Click(object sender, RoutedEventArgs e)
        {
            changeTab(Section.General);
        }

        private void rad_aspect_Click(object sender, RoutedEventArgs e)
        {
            changeTab(Section.Aspect);
        }

        private void rad_controls_Click(object sender, RoutedEventArgs e)
        {
            changeTab(Section.Controls);
        }

        private void rad_audio_Click(object sender, RoutedEventArgs e)
        {
            changeTab(Section.Audio);
        }

        private void rad_lyrics_Click(object sender, RoutedEventArgs e)
        {
            changeTab(Section.Lyrics);
        }

        public enum Section
        {
            General,
            Aspect,
            Controls,
            Audio,
            Lyrics
        }

        public void changeTab(Section tab)
        {
            tab_general.Visibility = Visibility.Collapsed;
            tab_aspect.Visibility = Visibility.Collapsed;
            tab_lyrics.Visibility = Visibility.Collapsed;

            switch (tab)
            {
                case Section.General:
                    tab_general.Visibility = Visibility.Visible;
                    break;
                case Section.Aspect:
                    tab_aspect.Visibility = Visibility.Visible;
                    break;
                case Section.Lyrics:
                    tab_lyrics.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
