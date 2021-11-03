using Lyrics_3_Tag_Editor.Services;
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
    public partial class ReplacePage : Window
    {
        TableManager tableManager = null;

        public ReplacePage(TableManager tableManager)
        {
            InitializeComponent();
            this.tableManager = tableManager;
        }

        private void tb_find_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Refresh();
            }
        }

        private void Refresh()
        {
            List<RowModel> list = new List<RowModel>();
            for (int i = 0; i < tableManager.getLyrics().Count; i++)
            {
                if (tableManager.getLyrics()[i].lyrics.Contains(tb_find.Text) == true || 
                    tableManager.getLyrics()[i].lyrics.Contains(tb_replace.Text) == true)
                {
                    list.Add(new RowModel(tableManager.getLyrics()[i]));
                }
            }

            dg_lyrics.ItemsSource = list;
        }

        private void tb_replace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tableManager.getLyrics() != null)
                {
                    if (tableManager.getLyrics().Count >= 0)
                    {
                        tableManager.Edit(dg_lyrics.SelectedIndex, tableManager.getLyrics()[dg_lyrics.SelectedIndex].lyrics.Replace(tb_find.Text, tb_replace.Text));
                        this.Refresh();
                    }
                }
            }
        }

        private void btn_redirect_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
