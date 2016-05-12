using CoursePublishing;
using System;
using System.Linq;
using System.Windows;

namespace YoutubeSync
{
    class YoutubeSync
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var CourseName = Publishing.GetCourseNameFromArgs(args);
            var Settings = Publishing.Courses[CourseName].LoadInitOrEdit<CourseSettings>(2).Youtube;

            var videos =
                (
                from guid in Publishing.Courses[CourseName].Load<Structure>().Items.VideoGuids()
                join video in Publishing.Common.LoadList<Video>() on guid equals video.Guid
                select video
                ).ToList();

           
            var clips = 
                Publishing.Common.LoadList<YoutubeClip>()
                .Where(z => z.Channel == Settings.Channel)
                .ToList();

            var relation = Publishing.Common.LoadList<VideoToYoutubeClip>();


            var model = new MatchModel();

            var innerBound =
               (from clip in clips
                join rel in relation on clip.Id equals rel.YoutubeId
                join video in videos on rel.Guid equals video.Guid
                select new { video, clip }
                ).ToList();

            var outerBound =
                (
                from clip in clips
                join rel in relation on clip.Id equals rel.YoutubeId
                select new { clip }
                ).ToList();

            foreach (var e in innerBound)
                model.Matches.Add(Tuple.Create(e.video, new YoutubeClipViewModel(e.clip)));

            foreach (var v in videos.Except(innerBound.Select(z => z.video)))
            {
                model.Videos.Add(v);
            }

            foreach (var y in clips.Except(outerBound.Select(z => z.clip)))
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
