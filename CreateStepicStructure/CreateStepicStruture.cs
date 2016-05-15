using CoursePublishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateStepicStructure
{
    class Program
    {

        static void CreateLessons(StepicData data, Structure structure, StepicSyncSettings settings, string CourseName)
        {
            foreach (var e in structure.Items.Sections().Where(z => z.Level == settings.LessonsLevel))
            {
                if (data.Lessons.ContainsKey(e.Guid)) continue;
                var lesson = StepicApi.Lesson.Create(new { title = e.Name });
                var id = lesson.Value<int>("id");
                data.Lessons[e.Guid] = id;
                Publishing.Courses[CourseName].Save(data);
                Console.WriteLine($"{e.Name} {data.Lessons[e.Guid]}");
                break;
            }

        }

        static void Main(string[] args)
        {
            StepicApi.Authorize();
            var CourseName = Publishing.GetCourseNameFromArgs(args);
            var settings = Publishing.Courses[CourseName].LoadInitOrEdit<CourseSettings>(3).Stepic;
            var stepicData = Publishing.Courses[CourseName].LoadOrInit<StepicData>();
            var structure = Publishing.Courses[CourseName].Load<Structure>();
            CreateLessons(stepicData, structure, settings, CourseName);
        }
    }
}
