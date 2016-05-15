using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class StepicData
    {
        public Dictionary<Guid, int> Videos = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> Steps = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> Lessons = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> Modules = new Dictionary<Guid, int>();
    }
}
