using CoursePublishing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YoutubeSync
{
    public class YoutubeClipViewModel
    {
        public YoutubeClip Clip { get; }
        public ICommand Open { get; }
        public YoutubeClipViewModel(YoutubeClip clip)
        {
            Clip = clip;
            Open = new RelayCommand(z => Process.Start("http://youtube.com/watch?v=" + Clip.Id));
        }
    }
}
