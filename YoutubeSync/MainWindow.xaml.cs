using CoursePublishing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YoutubeSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ok.Click += CmOk;
            cancel.Click += CmCancel;
            match.Click += CmMatch;
            unmatch.Click += CmUnmatch;
        }

        MatchModel model => (MatchModel)DataContext;

        private void CmUnmatch(object sender, RoutedEventArgs e)
        {
            if (matches.SelectedItem == null) return;
            var m = matches.SelectedItem as Tuple<Video, YoutubeClipViewModel>;
            model.Matches.Remove(m);
            model.Videos.Insert(0, m.Item1);
            model.Clips.Insert(0, m.Item2);
        }

        private void CmMatch(object sender, RoutedEventArgs e)
        {
            if (clips.SelectedItem == null) return;
            if (videos.SelectedItem == null) return;
            var v = videos.SelectedItem as Video;
            var c = clips.SelectedItem as YoutubeClipViewModel;
            model.Clips.Remove(c);
            model.Videos.Remove(v);
            model.Matches.Insert(0, Tuple.Create(v,c));
        }

        private void CmCancel(object sender, RoutedEventArgs e)
        {
            model.SaveRequest = false;
            Close();
        }

        private void CmOk(object sender, RoutedEventArgs e)
        {
            model.SaveRequest = false;
            Close();
        }
    }
}
