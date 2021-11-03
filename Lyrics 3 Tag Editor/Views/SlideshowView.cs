using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lyrics_3_Tag_Editor.Views
{
    public class SlideshowView
    {
        #region Variables

        private List<LyricsModel> lyrics = null;
        private String[] array = new String[9];
        private Layout layout = Layout.OneRow;
        private TextBlock[] rows = new TextBlock[9];
        private int lastArray = -1;

        #endregion

        #region Layout

        private Grid rootView = null;
        private Grid rowsView = null;

        #endregion

        public void createView()
        {
            this.rootView = new Grid()
            {
                Background = new SolidColorBrush(Colors.Transparent)
            };

            this.rowsView = new Grid()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center
            };

            for (int i = 0; i < 9; i++)
            {
                rowsView.RowDefinitions.Add(new RowDefinition());

                TextBlock content = new TextBlock() {
                    FontSize = (i == 4 ? 18 : 14),
                    Margin = new Thickness(i == 4 ? 10 : 5),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                rows[i] = content;
                rowsView.Children.Add(rows[i]);
                Grid.SetRow(rows[i], i);
            }

            this.rootView.Children.Add(rowsView);
        }

        public Grid getView()
        {
            return this.rootView;
        }

        public void Load(List<LyricsModel> lyrics)
        {
            this.lyrics = lyrics;
        }

        public void Update(TimeSpan position)
        {
            int position_seconds = (int)position.TotalSeconds;
            List<LyricsModel> biggest_time = new List<LyricsModel>();
            List<LyricsModel> lowest_time = new List<LyricsModel>();

            for (int i = 0; i < this.lyrics.Count; i++)
            {
                int array_seconds = (int)this.lyrics[i].time.getSpan().TotalSeconds;

                if (layout != Layout.OneRow)
                {
                    if (array_seconds < position_seconds)
                    {
                        lowest_time.Add(this.lyrics[i]);
                    }

                    if (array_seconds > position_seconds)
                    {
                        biggest_time.Add(this.lyrics[i]);
                    }
                }

                if (lastArray != position_seconds)
                {
                    if (array_seconds == position_seconds)
                    {
                        array[4] = this.lyrics[i].getLyrics();
                        lastArray = position_seconds;
                    }
                }
            }

            if (layout != Layout.OneRow)
            {
                for (int i = 0; i < 4; i++)
                {
                    int lowest_base = 0 + i;
                    int biggest_base = 5 + i;

                    if (lowest_time.Count < i)
                    {
                        array[lowest_base] = "[EMPTY]";
                    }
                    else
                    {
                        int index = lowest_time.FindIndex(model => model.getTime().getSec() == lowest_time.Max(obj => obj.getTime().getSec()));
                        if (index != -1)
                        {
                            array[lowest_base] = lowest_time[index].getLyrics();
                            lowest_time.RemoveAt(index);
                        }
                    }

                    if (biggest_time.Count < i)
                    {
                        array[biggest_base] = "[EMPTY]";
                    }
                    else
                    {
                        int index = biggest_time.FindIndex(model => model.getTime().getSec() == biggest_time.Min(obj => obj.getTime().getSec()));
                        if (index != -1)
                        {
                            array[biggest_base] = biggest_time[index].getLyrics();
                            biggest_time.RemoveAt(index);
                        }
                    }
                }
            }

            if (array[4] == null)
            {
                array[4] = "[EMPTY]";
            }

            switch (layout)
            {
                case Layout.OneRow:
                    this.Display(Rows.Center, array[4]);
                    break;
                case Layout.ThreeRows:
                    this.Display(Rows.BackFour, array[3]);
                    this.Display(Rows.Center, array[4]);
                    this.Display(Rows.ForOne, array[5]);
                    break;
                case Layout.SixRows:
                    this.Display(Rows.BackTwo, array[1]);
                    this.Display(Rows.BackThree, array[2]);
                    this.Display(Rows.BackFour, array[3]);
                    this.Display(Rows.Center, array[4]);
                    this.Display(Rows.ForOne, array[5]);
                    this.Display(Rows.ForTwo, array[6]);
                    this.Display(Rows.ForThree, array[7]);
                    break;
                case Layout.NineRows:
                    this.Display(Rows.BackOne, array[0]);
                    this.Display(Rows.BackTwo, array[1]);
                    this.Display(Rows.BackThree, array[2]);
                    this.Display(Rows.BackFour, array[3]);
                    this.Display(Rows.Center, array[4]);
                    this.Display(Rows.ForOne, array[5]);
                    this.Display(Rows.ForTwo, array[6]);
                    this.Display(Rows.ForThree, array[7]);
                    this.Display(Rows.ForFour, array[8]);
                    break;
            }
        }

        public void Clear()
        {
        }

        public void Display(Rows row, String text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                TextBlock block = rows[(int)row];
                block.Foreground = new SolidColorBrush(text == ("[EMPTY]") ? Colors.LightGray : Colors.DimGray);
                block.FontWeight = row == Rows.Center ? FontWeights.Bold : FontWeights.Normal;
                block.Text = text;
            }));
        }

        public void setLayout(Layout layout)
        {
            int[] hiddenRows = new int[] { };
            int[] visibleRows = new int[] { };
            int[] centerRows = new int[] { };

            switch (layout)
            {
                case Layout.OneRow:
                    hiddenRows = new int[] { 0, 1, 2, 3, 5, 6, 7, 8 };
                    visibleRows = new int[] { 4 };
                    centerRows = new int[] { 4 };
                    break;
                case Layout.ThreeRows:
                    hiddenRows = new int[] { 0, 1, 2, 6, 7, 8 };
                    visibleRows = new int[] { 3, 4, 5 };
                    centerRows = new int[] { 4 };

                    break;
                case Layout.SixRows:
                    hiddenRows = new int[] { 0, 1, 7, 8 };
                    visibleRows = new int[] { 2, 3, 4, 5, 6 };
                    centerRows = new int[] { 4 };
                    break;
                case Layout.NineRows:
                    hiddenRows = new int[] { };
                    visibleRows = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                    centerRows = new int[] { 4 };
                    break;
            }

            for (int i = 0; i < hiddenRows.Length; i++)
            {
                rowsView.RowDefinitions[hiddenRows[i]].Height = new GridLength(0, GridUnitType.Pixel);
            }

            for (int i = 0; i < visibleRows.Length; i++)
            {
                rowsView.RowDefinitions[visibleRows[i]].Height = new GridLength(10, GridUnitType.Star);
            }

            for (int i = 0; i < centerRows.Length; i++)
            {
                rowsView.RowDefinitions[centerRows[i]].Height = new GridLength(10, GridUnitType.Star);
            }

            this.layout = layout;
        }

        public enum Layout
        {
            [Description("1x")]
            OneRow,
            [Description("3x")]
            ThreeRows,
            [Description("6x")]
            SixRows,
            [Description("9x")]
            NineRows
        }

        public enum Rows
        {
            [Description("9x")]
            BackFour = 0,
            [Description("9x")]
            BackThree = 1,
            [Description("6x")]
            BackTwo = 2,
            [Description("3x")]
            BackOne = 3,
            [Description("1x")]
            Center = 4,
            [Description("3x")]
            ForOne = 5,
            [Description("6x")]
            ForTwo = 6,
            [Description("9x")]
            ForThree = 7,
            [Description("9x")]
            ForFour = 8
        }
    }
}