using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{

    public class Video
    {
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public string OriginalLocation { get; set; }
        public int EpisodeNumber { get; set; }
    }
}