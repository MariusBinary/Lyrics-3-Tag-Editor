using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lyrics_3_Tag_Editor.Utils;

namespace Lyrics_3_Tag_Editor.Services
{
    public class TableManager
    {
        #region Callback

        public Callback.onLyricsChanged onLyricsChanged { get; set; }

        public class Callback
        {
            public delegate void onLyricsChanged(List<LyricsModel> lyrics);
        }

        #endregion

        #region Variables

        private DataGrid table = null;
        private List<LyricsModel> lyrics = null;
        private ObservableRangeCollection<RowModel> rows = null;

        private int lastRow = -1;

        #endregion

        public TableManager(DataGrid table)
        {
            this.table = table;
            this.lyrics = new List<LyricsModel>();
            this.rows = new ObservableRangeCollection<RowModel>();
            this.table.ItemsSource = rows;
        }

        public void Load(List<LyricsModel> lyrics)
        {
            this.lyrics = lyrics;
            this.Refresh();
        }

        public void Import(List<LyricsModel> lyrics)
        {
            this.lyrics.AddRange(lyrics);
            this.Refresh();
        }

        public void Add()
        {
            LyricsModel model = new LyricsModel();
            this.lyrics.Add(model);
            this.rows.Add(new RowModel(model));
            this.onLyricsChanged?.Invoke(this.lyrics);
        }

        public void Remove(int index)
        {
            this.lyrics.RemoveAt(index);
            this.rows.RemoveAt(index);
            this.onLyricsChanged?.Invoke(this.lyrics);
        }

        public void RemoveAll()
        {
            this.lyrics.Clear();
            this.rows.Clear();
            this.onLyricsChanged?.Invoke(this.lyrics);
        }

        public void Clear(int index)
        {
            this.lyrics[index].Clear();
            this.rows[index] = new RowModel(this.lyrics[index]);
            this.onLyricsChanged?.Invoke(this.lyrics);
        }

        public void Sort(TimeSpan time)
        {
            this.lyrics = LyricsUtils.Sort(lyrics, time);
            this.Refresh();
        }

        public void Edit(int index, TimeSpan time)
        {
            this.lyrics[index].setTime(time);
            this.rows[index].setTime(this.lyrics[index].getTime());
            this.onLyricsChanged?.Invoke(this.lyrics);
        }

        public void Edit(int index, String text)
        {
            this.lyrics[index].setLyrics(text);
            this.rows[index].setLyrics(text);
            this.onLyricsChanged?.Invoke(this.lyrics);
        }

        public void Refresh()
        {
            this.rows.Clear();
            for (int i = 0; i < this.lyrics.Count; i++) {
                this.rows.Add(new RowModel(this.lyrics[i]));
            }
            this.table.Items.Refresh();
            this.onLyricsChanged?.Invoke(this.lyrics);
        }

        public void LostFocus()
        {
            try
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;
                table.MoveFocus(tRequest);
            }
            catch
            {
            }
        }

        public void setRow(int rowIndex)
        {
            this.table.Focus();
            this.table.CurrentCell = new DataGridCellInfo(table.Items[rowIndex], table.Columns[0]);
            this.table.ScrollIntoView(table.Items[rowIndex]);
            this.table.SelectedIndex = rowIndex;
        }

        public void setFocus(int rowIndex, bool focus)
        {
            if (focus == true)
            {
                this.table.Focus();
                this.table.CurrentCell = new DataGridCellInfo(table.Items[rowIndex], table.Columns[0]);
                this.table.ScrollIntoView(table.Items[rowIndex]);
                this.table.SelectedIndex = rowIndex;
            }
            else
            {
                this.table.CurrentCell = new DataGridCellInfo(table.Items[rowIndex], table.Columns[0]);

            }
        }

        public void setSelectedRow(int row)
        {
            lastRow = row;
        }

        public List<LyricsModel> getLyrics()
        {
            return lyrics;
        }

        public int getRows()
        {
            return rows.Count;
        }

        public int getSelectedRow()
        {
            return table.SelectedIndex;
        }

        public int getLastRow()
        {
            return table.SelectedIndex;
        }
    }
}