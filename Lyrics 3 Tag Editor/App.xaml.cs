using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Lyrics_3_Tag_Editor.Utils;


namespace Lyrics_3_Tag_Editor
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            String culture = CultureUtils.Language.Get();
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);

            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "en-US":
                    dict.Source = new Uri("..\\Culture\\Resources.en-US.xaml", UriKind.Relative);
                    break;
                case "it-IT":
                    dict.Source = new Uri("..\\Culture\\Resources.it-IT.xaml", UriKind.Relative);
                    break;
                case "ru-RU":
                    dict.Source = new Uri("..\\Culture\\Resources.ru-RU.xaml", UriKind.Relative);
                    break;
                case "ro-RO":
                    dict.Source = new Uri("..\\Culture\\Resources.ro-RO.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Culture\\Resources.en-US.xaml", UriKind.Relative);
                    break;
            }

            this.Resources.MergedDictionaries.Add(dict);
            base.OnStartup(e);
        }
    }
}
