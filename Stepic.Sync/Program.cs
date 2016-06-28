using CoursePublishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stepic.Sync
{
    class Program
    {
        static void MoveLesson()
        {

            var CourseName = "LHPS";
            var StepicData = Publishing.Courses[CourseName].LoadOrInit<StepicData>();
            StepicApi.Authorize();
            var whatGuid = Guid.Parse("aeccd8fc-f968-4832-b869-74761ce11602");
            var fromGuid = Guid.Parse("57e38135-5283-4a31-a9a7-476ae12cfb92");
            var toGuid = Guid.Parse("85f2a23b-ce76-4361-b9da-8890f2a8a1b2");
            var unit = StepicData.Units
                .Where(z => z.LessonGuid == whatGuid && z.SectionGuid == fromGuid)
                .First();
            StepicApi.Units.Delete(unit.UnitId.ToString());
            var newUnit = StepicApi.Units.Create(new
            {
                lesson = StepicData.Lessons[whatGuid],
                section = StepicData.Sections[toGuid],
                position = 2,
            });
            var id = 10509; //newUnit.Value<int>("id");
            StepicData.Units.Add(new StepicUnit { LessonGuid = whatGuid, SectionGuid = toGuid, UnitId = id });
            StepicData.Units.Remove(unit);
            Publishing.Courses[CourseName].Save(StepicData);
        }

        static void Main(string[] args)
        {
            var CourseName = "LHPS";
            var StepicData = Publishing.Courses[CourseName].LoadOrInit<StepicData>();
            var deleteId = Guid.Parse("57e38135-5283-4a31-a9a7-476ae12cfb92");
            StepicApi.Authorize();
            StepicApi.Section.Delete(StepicData.Sections[deleteId]);
            StepicData.Sections.Remove(deleteId);
            Publishing.Courses[CourseName].Save(StepicData);
        }
    }
}