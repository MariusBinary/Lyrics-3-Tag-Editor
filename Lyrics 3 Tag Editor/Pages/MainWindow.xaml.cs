using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Text;
using NAudio.Wave;
using System.Collections.Generic;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Lyrics_3_Tag_Editor.Utils;
using System.Collections.Specialized;
using Lyrics_3_Tag_Editor.Services;
using Lyrics_3_Tag_Editor.Views;
using System.Windows.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Lyrics_3_Tag_Editor.Pages
{
    public partial class MainWindow : Window
    {
        #region Variables

        public MediaManager mediaManager = null;
        public AudioPlayer audioPlayer = null;
        public LyricsManager lyricsManager = null;
        public TableManager tableManager = null;
        public SlideshowView slideshowView = null;

        public FileModel file = null;
        public List<LyricsModel> lyrics = null;

        public List<string> recents = null;

        #endregion

        #region Window

        private String baseTitle = null;
        private UIElement[] uIElements = null;

        public MainWindow()
        {
            InitializeComponent();

            baseTitle = Title + ((System.Runtime.InteropServices.Marshal.SizeOf(IntPtr.Zero) == 8) ? " (x64)" : " (x86)");
            Title = baseTitle;

            uIElements = new UIElement[] {
                btn_add,
                btn_remove,
                btn_clear,
                btn_sort,
                btn_remove_all,
                tab_player,
                tb_position,
                seek_position,
                item_export,
                item_import,
                item_refresh,
                item_save,
                item_save_as,
                item_go_to,
                item_find,
                item_replace,
                dg_lyrics
            };

            this.mediaManager = new MediaManager();
            this.mediaManager.onMediaLoaded += mediaManager_onMediaLoaded;
            this.mediaManager.onMediaCleaned += mediaManager_onMediaCleaned;

            this.audioPlayer = new AudioPlayer();
            this.audioPlayer.onMediaLoaded += audioPlayer_onMediaLoaded;
            this.audioPlayer.onPlay += audioPlayer_onPlay;
            this.audioPlayer.onPause += audioPlayer_onPause;
            this.audioPlayer.onStop += audioPlayer_onStop;
            this.audioPlayer.onPositionChanged += audioPlayer_onPositionChanged;

            this.lyricsManager = new LyricsManager();
            this.lyricsManager.onMediaLoaded += lyricsManager_onMediaLoaded;
            this.lyricsManager.onMediaSaved += lyricsManager_onMediaSaved;

            this.tableManager = new TableManager(dg_lyrics);
            this.tableManager.onLyricsChanged += tableManager_onLyricsChanged;

            this.slideshowView = new SlideshowView();
            this.slideshowView.createView();
            this.slideshow_content.Children.Add(slideshowView.getView());

            setElements(0);
            setHistory();
            setPreferences();
            setWindow();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.audioPlayer.Stop();
            this.audioPlayer.Clear();
            this.lyricsManager.Clear();
            this.mediaManager.Clear();
            Environment.Exit(0);
        }
    
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (mediaManager.IsMediaPresent == true)
            {
                if (e.Key == System.Windows.Input.Key.MediaPlayPause)
                {
                    if (this.audioPlayer.getPlaybackState() == PlaybackState.Playing)
                    {
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(btn_pause);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                    }
                    else if (this.audioPlayer.getPlaybackState() == PlaybackState.Paused)
                    {
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(btn_play);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                    }
                    else if (this.audioPlayer.getPlaybackState() == PlaybackState.Stopped)
                    {
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(btn_play);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                    }
                }
                else if (e.Key == System.Windows.Input.Key.MediaStop)
                {
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(btn_stop);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv.Invoke();
                }
                else if (e.Key == System.Windows.Input.Key.MediaNextTrack)
                {
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(btn_down);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv.Invoke();
                }
                else if (e.Key == System.Windows.Input.Key.MediaPreviousTrack)
                {
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(btn_up);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv.Invoke();
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Save();
        }

        public void setElements(int status)
        {
            if (status == 0)
            {
                for (int i = 0; i < uIElements.Length; i++)
                {
                    uIElements[i].IsEnabled = false;
                }
            }
            else
            {
                for (int i = 0; i < uIElements.Length; i++)
                {
                    uIElements[i].IsEnabled = true;
                }
            }

            dropAnimation = (Storyboard)this.FindResource("DropAnimation");
        }

        public void setTable(int status)
        {
            col_status.Visibility = status == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public void setHistory()
        {
            StringCollection history = Properties.Settings.Default.History;
            item_recent.Items.Clear();

            if (history != null)
            {
                if (history.Count >= 0)
                {
                    for (int i = 0; i < history.Count; i++)
                    {
                        MenuItem item = new MenuItem();
                        item.Header = history[i];
                        item.Click += (sender, e) => {
                            MenuItem obj = (MenuItem)sender;
                            Open(obj.Header.ToString());
                        };
                        item_recent.Items.Add(item);
                    }

                    item_recent.IsEnabled = true;
                    return;
                }
            }

            item_recent.IsEnabled = false;
        }

        public void setWindow()
        {
            this.Width = Properties.Settings.Default.Width;
            this.Height = Properties.Settings.Default.Height;
        }

        public void setPreferences()
        {
            seek_volume.Value = Properties.Settings.Default.Volume;

            switch (Properties.Settings.Default.SlideshowLayout)
            {
                case 0:
                    rad_lyrics_1x.IsChecked = true;
                    slideshowView.setLayout(SlideshowView.Layout.OneRow);
                    break;
                case 1:
                    rad_lyrics_2x.IsChecked = true;
                    slideshowView.setLayout(SlideshowView.Layout.ThreeRows);
                    break;
                case 2:
                    rad_lyrics_4x.IsChecked = true;
                    slideshowView.setLayout(SlideshowView.Layout.SixRows);
                    break;
                case 3:
                    rad_lyrics_8x.IsChecked = true;
                    slideshowView.setLayout(SlideshowView.Layout.NineRows);
                    break;
            }

            switch (Properties.Settings.Default.Layout)
            {
                case 0:
                    tab_content.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    tab_content.ColumnDefinitions[1].Width = new GridLength(10, GridUnitType.Pixel);
                    tab_content.ColumnDefinitions[2].Width = new GridLength(565, GridUnitType.Pixel);

                    Grid.SetColumn(tab_display, 2);
                    Grid.SetColumn(tab_table, 0);
                    break;
                case 1:
                    tab_content.ColumnDefinitions[0].Width = new GridLength(565, GridUnitType.Pixel);
                    tab_content.ColumnDefinitions[1].Width = new GridLength(10, GridUnitType.Pixel);
                    tab_content.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);

                    Grid.SetColumn(tab_display, 0);
                    Grid.SetColumn(tab_table, 2);
                    break;
                case 2:
                    tab_content.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    tab_content.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);
                    tab_content.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Pixel);

                    Grid.SetColumn(tab_display, 2);
                    Grid.SetColumn(tab_table, 0);
                    break;
            }

            setTable(Properties.Settings.Default.AllowRowStatus == true ? 1 : 0);
            this.tableManager.Refresh();
        }

        #endregion

        #region Callback

        public void mediaManager_onMediaLoaded(String f1, String f2)
        {
            file = new FileModel(f1, f2);

            if (this.audioPlayer != null)
            {
                this.audioPlayer.Stop();
                this.audioPlayer.Clear();

                StringCollection history = Properties.Settings.Default.History;

                if (history != null)
                {
                    if (history.Count >= 0)
                    {
                        if (history.Contains(file.Name))
                        {
                            history.Remove(file.Name);
                        }

                        if (history.Count >= 15)
                        {
                            history.RemoveAt(14);
                            history.Insert(0, file.Name);
                        }
                        else
                        {
                            history.Insert(0, file.Name);
                        }
                    }
                }
                else
                {
                    history = new StringCollection();
                    history.Insert(0, file.Name);
                }

                Properties.Settings.Default.History = history;
                Properties.Settings.Default.Save();
                setHistory();

                switch (this.audioPlayer.Load(file))
                {
                    case 0:
                        this.setElements(1);
                        this.onMediaChanged();
                        break;
                    case 1:
                        MessageBox.Show(CultureUtils.Get("msg_player_plugin_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                    case 2:
                        MessageBox.Show(CultureUtils.Get("msg_player_inizializing_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                    case 3:
                        MessageBox.Show(CultureUtils.Get("msg_player_file_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                    case 4:
                        MessageBox.Show(CultureUtils.Get("msg_player_output_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                }
            }
        }

        public void mediaManager_onMediaCleaned()
        {

        }

        public void audioPlayer_onMediaLoaded(FileModel file)
        {
            if (this.lyricsManager != null)
            {
                if (!this.lyricsManager.Load(file))
                {
                    MessageBox.Show(CultureUtils.Get("msg_lyrics_reading_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void audioPlayer_onPositionChanged(AudioFileReader fileReader, TimeSpan position)
        {
            seek_position.Value = (int)(1000 * position.TotalSeconds / fileReader.TotalTime.TotalSeconds);
            tb_position.Text = TimeUtils.Convert.ToText(position);

            if (Properties.Settings.Default.Layout != 2)
            {
                slideshowView.Update(position);
            }
        }

        public void audioPlayer_onPlay()
        {

        }

        public void audioPlayer_onPause()
        {

        }

        public void audioPlayer_onStop()
        {
            slideshowView.Clear();
            seek_position.Value = 0;
            tb_position.Text = "00:00";
        }

        public void lyricsManager_onMediaLoaded(List<LyricsModel> lyrics)
        {
            if (oldLyrics != null)
            {
                if (this.lyricsManager.Save(oldLyrics) == true)
                {
                    MessageBox.Show(CultureUtils.Get("msg_file_success_saved"), CultureUtils.Get("msg_type_info"), MessageBoxButton.OK, MessageBoxImage.Information);

                    if (this.tableManager != null)
                    {
                        this.tableManager.Load(oldLyrics);
                        return;
                    }
                }

                oldLyrics = null;
            }
            else
            {
                if (this.tableManager != null)
                {
                    this.tableManager.Load(lyrics);
                }
            }
        }

        public void lyricsManager_onMediaSaved()
        {

        }

        public void tableManager_onLyricsChanged(List<LyricsModel> lyrics)
        {
            if (file != null)
            {
                this.lyrics = lyrics;
                slideshowView.Load(lyrics);
            }
        }

        #endregion

        #region Global Callbacks

        public void onMediaChanged()
        {
            Title = baseTitle + " | " + file.Name;
            tb_info_title.Text = file.Name;
            tb_info_bitrate.Text = file.Bitrate;
            tb_info_frequency.Text = file.Frequency;

            tb_tag_title.Text = file.Tag.Title;
            tb_tag_artist.Text = file.Tag.Artist;
            tb_tag_album.Text = file.Tag.Album;
            tb_tag_track.Text = file.Tag.Track.ToString();
            tb_tag_year.Text = file.Tag.Year.ToString();
            img_tag_art.Source = file.Tag.CovertArt;
        }

        #endregion

        #region Menu

        #region File

        private void item_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 Files (*.mp3*)|*.mp3*";
            if (openFileDialog.ShowDialog() == true)
            {
                Open(openFileDialog.FileName);
            }
        }

        public void Open(String filename)
        {
            if (this.mediaManager != null)
            {
                this.audioPlayer.Stop();
                this.audioPlayer.Clear();
                this.lyricsManager.Clear();
                this.mediaManager.Clear();

                switch (this.mediaManager.Load(filename))
                {
                    case 0:
                        break;
                    case 1:
                        MessageBox.Show(CultureUtils.Get("msg_temp_file_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                    case 2:
                        MessageBox.Show(CultureUtils.Get("msg_readonly_file_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                    case 3:
                        MessageBox.Show(CultureUtils.Get("msg_readonly_remove_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                }
            }
        }

        private void item_save_Click(object sender, RoutedEventArgs e)
        {
            if (this.tableManager != null)
            {
                this.tableManager.LostFocus();
            }

            if (this.lyricsManager.Save(this.lyrics) == true)
            {
                MessageBox.Show(CultureUtils.Get("msg_file_success_saved"), CultureUtils.Get("msg_type_info"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(CultureUtils.Get("msg_file_failed_saved"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        List<LyricsModel> oldLyrics = null;
        private void item_save_as_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MP3 Files (*.mp3*)|*.mp3*";
            if (saveFileDialog.ShowDialog() == true)
            {
                oldLyrics = this.lyrics;
                switch (this.mediaManager.Load(saveFileDialog.FileName))
                {
                    case 0:
                        break;
                    case 1:
                        MessageBox.Show(CultureUtils.Get("msg_temp_file_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                    case 2:
                        MessageBox.Show(CultureUtils.Get("msg_readonly_file_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                    case 3:
                        MessageBox.Show(CultureUtils.Get("msg_readonly_remove_error"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        setElements(0);
                        break;
                }
            }
        }

        private void item_settings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsPage(this).ShowDialog();
            this.setPreferences();
        }

        private void item_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Lyrics

        private void item_import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TXT Files (*.txt*)|*.txt*";
            if (openFileDialog.ShowDialog() == true)
            {
                String filename = openFileDialog.FileName;
                String text = System.IO.File.ReadAllText(filename, Encoding.Default);

                if (this.lyrics.Count >= 0)
                {
                    if (MessageBox.Show(CultureUtils.Get("msg_delete_present_lyrics"), CultureUtils.Get("msg_type_confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        this.tableManager.Load(LyricsUtils.ToArray(text, Properties.Settings.Default.AllowImportBlankLines == true ? false : true, Properties.Settings.Default.AllowImportTime));
                        return;
                    }
                }

                this.tableManager.Import(LyricsUtils.ToArray(text, Properties.Settings.Default.AllowImportBlankLines == true ? false : true, Properties.Settings.Default.AllowImportTime));
            }
        }

        private void item_export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "TXT Files (*.txt*)|*.txt*";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    String filename = saveFileDialog.FileName;
                    System.IO.File.WriteAllText(filename, LyricsUtils.ToString(this.tableManager.getLyrics(), Properties.Settings.Default.AllowExportTime));
                    MessageBox.Show(CultureUtils.Get("msg_file_success_exported"), CultureUtils.Get("msg_type_info"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show(CultureUtils.Get("msg_file_not_exported"), CultureUtils.Get("msg_type_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void item_refresh_Click(object sender, RoutedEventArgs e)
        {
            if (this.tableManager != null)
            {
                this.tableManager.LostFocus();
                this.tableManager.Refresh();
            }
        }

        #endregion

        #region Informations

        #region Languages

        private void item_lang_en_Click(object sender, RoutedEventArgs e)
        {
            CultureUtils.Language.Set("en-US");
        }

        private void item_lang_it_Click(object sender, RoutedEventArgs e)
        {
            CultureUtils.Language.Set("it-IT");
        }

        private void item_lang_ru_Click(object sender, RoutedEventArgs e)
        {
            CultureUtils.Language.Set("ru-RU");
        }

        private void item_lang_ro_Click(object sender, RoutedEventArgs e)
        {
            CultureUtils.Language.Set("ro-RO");
        }

        #endregion

        private void item_about_software_Click(object sender, RoutedEventArgs e)
        {
            new AboutPage().ShowDialog();
        }

        #endregion

        #endregion

        #region Player

        private bool isSeekPositionDragged = false;

        private void seek_position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.audioPlayer != null)
            {
                if (isSeekPositionDragged == true)
                {
                    this.audioPlayer.setPosition(seek_position.Value);
                }
            }
        }

        private void seek_position_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (this.audioPlayer != null)
            {
                this.audioPlayer.Pause();
                isSeekPositionDragged = true;
            }
        }

        private void seek_position_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (this.audioPlayer != null)
            {
                this.audioPlayer.Play();
                isSeekPositionDragged = false;
            }
        }

        private void seek_volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.audioPlayer != null)
            {
                this.audioPlayer.setVolume((float)seek_volume.Value);
            }
        }

        private void seek_volume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Properties.Settings.Default.Volume = (float)seek_volume.Value;
            Properties.Settings.Default.Save();
        }

        private void btn_volume_Click(object sender, RoutedEventArgs e)
        {
            seek_volume.Value = seek_volume.Value > 0 ? 0 : Properties.Settings.Default.Volume;
        }

        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            if (this.audioPlayer != null)
            {
                this.audioPlayer.Play();
            }
        }

        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {
            if (this.audioPlayer != null)
            {
                this.audioPlayer.Pause();
            }
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            if (this.audioPlayer != null)
            {
                this.audioPlayer.Stop();
            }
        }

        #endregion

        #region Lyrics

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            this.tableManager.Add();
        }

        private void btn_remove_Click(object sender, RoutedEventArgs e)
        {
            int selectedRow = this.tableManager.getSelectedRow();
            if (selectedRow != -1)
            {
                this.tableManager.Remove(selectedRow);
            }
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            int selectedRow = this.tableManager.getSelectedRow();
            if (selectedRow != -1)
            {
                this.tableManager.Clear(selectedRow);
            }
        }

        private void btn_sort_Click(object sender, RoutedEventArgs e)
        {
            this.tableManager.Sort(file.Duration);
        }

        private void btn_remove_all_Click(object sender, RoutedEventArgs e)
        {
            if (this.tableManager.getRows() != -1)
            {
                this.tableManager.RemoveAll();
            }
        }

        private void btn_sync_Click(object sender, RoutedEventArgs e)
        {
            int selectedRow = this.tableManager.getSelectedRow();
            if (selectedRow != -1)
            {
                this.tableManager.Edit(selectedRow, this.audioPlayer.getPosition());
                if (selectedRow + 1 <= this.tableManager.getLyrics().Count - 1)
                {
                    this.tableManager.setRow(selectedRow + 1);
                }
            }
        }

        private void btn_sync_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void btn_up_Click(object sender, RoutedEventArgs e)
        {
            int selectedRow = this.tableManager.getSelectedRow();
            if (selectedRow != -1 && (selectedRow - 1) > -1)
            {
                this.tableManager.setRow(selectedRow - 1);
            }
        }

        private void btn_down_Click(object sender, RoutedEventArgs e)
        {
            int countRows = this.tableManager.getRows();
            int selectedRow = this.tableManager.getSelectedRow();
            if (selectedRow != -1 && (selectedRow + 1) <= countRows - 1)
            {
                this.tableManager.setRow(selectedRow + 1);
            }
            else if (selectedRow != -1 && countRows >= 0)
            {
                this.tableManager.setRow(countRows - 1);
            }
            else
            {
                if (countRows >= 0)
                {
                    this.tableManager.setRow(0);
                }
            }
        }

        #region Slideshow 

        private void rad_lyrics_1x_Click(object sender, RoutedEventArgs e)
        {
            slideshowView.setLayout(SlideshowView.Layout.OneRow);
            Properties.Settings.Default.SlideshowLayout = 0;
            Properties.Settings.Default.Save();
        }

        private void rad_lyrics_2x_Click(object sender, RoutedEventArgs e)
        {
            slideshowView.setLayout(SlideshowView.Layout.ThreeRows);
            Properties.Settings.Default.SlideshowLayout = 1;
            Properties.Settings.Default.Save();
        }

        private void rad_lyrics_4x_Click(object sender, RoutedEventArgs e)
        {
            slideshowView.setLayout(SlideshowView.Layout.SixRows);
            Properties.Settings.Default.SlideshowLayout = 2;
            Properties.Settings.Default.Save();
        }

        private void rad_lyrics_8x_Click(object sender, RoutedEventArgs e)
        {
            slideshowView.setLayout(SlideshowView.Layout.NineRows);
            Properties.Settings.Default.SlideshowLayout = 3;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Table

        string oldTableTime = "";
        string oldTableLyrics = "";

        private void dg_time_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            String value = obj.Text.Replace("[", "").Replace("]", "");

            if (oldTableTime != obj.Text)
            {
                if (TimeUtils.Validate(value) == true)
                {
                    obj.Text = value;
                    this.tableManager.Edit(this.tableManager.getSelectedRow(), TimeUtils.Convert.ToTime(value));
                }
                else
                {
                    obj.Text = oldTableTime;
                }
            }
        }

        private void dg_time_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            oldTableTime = obj.Text;
        }

        private void dg_lyrics_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            if (oldTableLyrics != obj.Text)
            {
                this.tableManager.Edit(this.tableManager.getSelectedRow(), obj.Text);
            }
        }

        private void dg_lyrics_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            oldTableLyrics = obj.Text;
        }

        private void dg_lyrics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tableManager.setSelectedRow(this.tableManager.getSelectedRow());
        }

        #endregion

        #endregion

        private void item_replace_Click(object sender, RoutedEventArgs e)
        {
            new ReplacePage(this.tableManager).ShowDialog();
        }

        private void item_find_Click(object sender, RoutedEventArgs e)
        {
            new FindPage(this.lyrics).ShowDialog();
        }

        #region Drag and Drop 

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
        }

        Storyboard dropAnimation = null;
        Boolean allowDrop = false;

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (allowDrop == true)
            {
                string[] files = (e.Data.GetData(DataFormats.FileDrop) as string[]);
                foreach (string file in files)
                {
                    if (file.EndsWith(".mp3"))
                    {
                        this.Open(file);
                        break;
                    }
                }
            }

            dropAnimation.Stop();
            tab_drop.Visibility = Visibility.Collapsed;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            tab_drop.Visibility = Visibility.Collapsed;
            dropAnimation.Stop();
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (Properties.Settings.Default.AllowFilesDragAndDrop == true)
            {
                if (e.Data.GetDataPresent(DataFormats.Text) == true ||
                    e.Data.GetDataPresent(DataFormats.OemText) == true ||
                    e.Data.GetDataPresent(DataFormats.UnicodeText) == true ||
                    e.Data.GetDataPresent(DataFormats.StringFormat) == true)
                {
                    allowDrop = false;
                    tab_drop.Visibility = Visibility.Collapsed;
                    dropAnimation.Stop();
                }
                else
                {
                    allowDrop = true;
                    tab_drop.Visibility = Visibility.Visible;
                    dropAnimation.Begin();
                }
            }
            else
            {
                allowDrop = false;
                tab_drop.Visibility = Visibility.Collapsed;
                dropAnimation.Stop();
            }
        }

        #endregion

        private void item_go_to_Click(object sender, RoutedEventArgs e)
        {
            new GoToPage(this.lyrics).ShowDialog();
        }
    }
}
