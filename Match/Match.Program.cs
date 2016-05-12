using CoursePublishing;
using System;
using System.Linq;
using System.Windows;

namespace YoutubeSync
{
    class YoutubeSync
    {


        [STAThread]
        public static void Main()
        {


            var clips = Publishing.Common.LoadList<YoutubeClip>();
            var relation = Publishing.Common.LoadList<VideoToYoutubeClip>();
            var videos = Publishing.Common.LoadList<Video>();

            var model = new MatchModel();

            var bound =
               (from clip in clips
                join rel in relation on clip.Id equals rel.YoutubeId
                join video in videos on rel.Guid equals video.Guid
                select new { video, clip }
                ).ToList();

            foreach (var e in bound)
                model.Matches.Add(Tuple.Create(e.video, new YoutubeClipViewModel(e.clip)));

            foreach (var v in videos.Except(bound.Select(z => z.video)))
            {
                model.Videos.Add(v);
            }

            foreach (var y in clips.Except(bound.Select(z => z.clip)))
            {
                model.Clips.Add(new YoutubeClipViewModel(y));
            }

            var window = new MainWindow();
            window.DataContext = model;
            (new Application()).Run(window);

            if (model.SaveRequest)
            {
                relation = model.Matches.Select(z => new VideoToYoutubeClip(z.Item1.Guid, z.Item2.Clip.Id)).ToList();
                Publishing.Common.UpdateList(relation,z=>z.Guid.ToString());
            }
        }
    }
}
