using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class VideoToYoutubeClip
    {
        public readonly Guid Guid;
        public readonly string YoutubeId;
        public VideoToYoutubeClip(Guid guid, string youtubeId)
        {
            Guid = guid;
            YoutubeId = youtubeId;
        }
    }
}
