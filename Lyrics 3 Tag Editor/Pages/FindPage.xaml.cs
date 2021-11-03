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
    public partial class FindPage : Window
    {
        private List<LyricsModel> lyrics = null;

        public FindPage(List<LyricsModel> lyrics)
        {
            InitializeComponent();
            this.lyrics = lyrics;
        }

        private void tb_find_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                List<RowModel> list = new List<RowModel>();
                for (int i = 0; i < lyrics.Count; i++)
                {
                    if (lyrics[i].lyrics.Contains(tb_find.Text) == true)
                    {
                        list.Add(new RowModel(lyrics[i]));
                    }
                }
                dg_lyrics.ItemsSource = list;
            }
        }

        private void btn_redirect_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
