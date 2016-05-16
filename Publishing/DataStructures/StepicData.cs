using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class StepicUnit
    {
        public Guid SectionGuid { get; set; }
        public Guid LessonGuid { get; set; }
        public int UnitId { get; set; }
    }


    public class StepicData
    {
        public Dictionary<Guid, int> Videos = new Dictionary<Guid, int>();
        public Dictionary<Guid, string> Thumbnails = new Dictionary<Guid, string>();
        public Dictionary<Guid, int> Steps = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> Lessons = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> Sections = new Dictionary<Guid, int>();
        public List<StepicUnit> Units = new List<StepicUnit>();
    }
}
