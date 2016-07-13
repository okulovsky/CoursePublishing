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
        const string CourseName = "LHPS";
        static StepicData StepicData;

        static void Save()
        {
            Publishing.Courses[CourseName].Save(StepicData);
        }

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

        static void DeleteLesson(Guid guid)
        {
            StepicApi.Lesson.Delete(StepicData.Lessons[guid]);
            StepicData.Lessons.Remove(guid);
            Publishing.Courses[CourseName].Save(StepicData);
        }

        static void CreateLesson(Guid lessonGuid)
        {
            var structure = Publishing.Courses[CourseName].Load<Structure>();
            if (StepicData.Lessons.ContainsKey(lessonGuid)) throw new Exception("Already created");

            var lesson = structure.Items.Sections().Where(z => z.Guid == lessonGuid).First();
            var lessonAtStepic = StepicApi.Lesson.Create(new { title = lesson.Name });
            var lessonId = lessonAtStepic.Value<int>("id");
            StepicData.Lessons[lesson.Guid] = lessonId;
            Publishing.Courses[CourseName].Save(StepicData);
            Console.WriteLine($"{lesson.Name} {StepicData.Lessons[lesson.Guid]}");

            var videos = lesson.Items.VideoGuids().ToList();

            for (int index = 0; index < videos.Count; index++)
            {
                var position = index + 1;
                var step = StepicApi.Step.Create(
                   new
                   {
                       block = new
                       {
                           text = "",
                           name = "video",
                           video = new
                           {
                               id = StepicData.Videos[videos[index]].ToString(),
                               status = "raw",
                               thumbnail = StepicData.Thumbnails[videos[index]].ToString(),
                               urls = new string[] { }
                           }
                       },
                       position = position,
                       lesson = lessonId
                   });
                StepicData.Steps[videos[index]] = int.Parse(step.Value<string>("id"));
                Save();
            }

            var section = structure.Items.Sections().Where(z => z.Sections.Contains(lesson)).First();

            var unit = new
            {
                lesson = lessonId,
                section = StepicData.Sections[section.Guid],
                position = section.Sections.IndexOf(lesson)+1,
            };

            var r = StepicApi.Units.Create(unit);
            var unitId = r.Value<int>("id");
            StepicData.Units.Add(new StepicUnit { LessonGuid = lesson.Guid, SectionGuid = section.Guid, UnitId = lessonId });
            Save();
        }

        static void Main(string[] args)
        {
            StepicData = Publishing.Courses[CourseName].LoadOrInit<StepicData>();
            StepicApi.Authorize();
            CreateLesson(Guid.Parse("4b178ac0-f1bb-4c93-adac-91d44df5b7a8"));
        }
    }
}