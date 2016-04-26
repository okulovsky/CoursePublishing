using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class VideoToCourse
    {
        public Guid CourseGuid { get; set; }
        public List<Guid> VideoGuids { get; private set; }
        public VideoToCourse()
        {
            VideoGuids = new List<Guid>();
        }
    }
}
