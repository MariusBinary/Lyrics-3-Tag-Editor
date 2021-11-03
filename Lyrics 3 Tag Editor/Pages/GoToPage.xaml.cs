using Lyrics_3_Tag_Editor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lyrics_3_Tag_Editor.Pages
{
    public partial class GoToPage : Window
    {
        private List<LyricsModel> lyrics = null;

        public GoToPage(List<LyricsModel> lyrics)
        {
            InitializeComponent();
            this.lyrics = lyrics;
        }

        private void tb_find_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String data = tb_find.Text.Replace(" ", "").Replace("[", "").Replace("]", "");
                if (TimeUtils.Validate(data) == true)
                {
                    List<RowModel> list = new List<RowModel>();
                    for (int i = 0; i < lyrics.Count; i++)
                    {
                        if (lyrics[i].time.getSpan().Equals(TimeUtils.Convert.ToTime(data)))
                        {
                            list.Add(new RowModel(lyrics[i]));
                        }
                    }
                    dg_lyrics.ItemsSource = list;
                }
            }
        }

        private void btn_redirect_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
