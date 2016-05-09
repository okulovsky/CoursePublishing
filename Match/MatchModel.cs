using CoursePublishing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSync
{
    class MatchModel
    {
        public ObservableCollection<YoutubeClipViewModel> Clips { get; }
        public ObservableCollection<Video> Videos { get; }
        public ObservableCollection<Tuple<Video, YoutubeClipViewModel>> Matches { get; }
        public bool SaveRequest { get; set; }

        public MatchModel()
        {
            Videos = new ObservableCollection<Video>();
            Clips = new ObservableCollection<YoutubeClipViewModel>();
            Matches = new ObservableCollection<Tuple<Video, YoutubeClipViewModel>>();
        }
    }
}
