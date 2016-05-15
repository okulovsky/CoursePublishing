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

        static void ClearLessons(StepicData data)
        {
            var lessons = StepicApi.Lesson.Get(new { teacher = 1133195 });
            foreach (var e in lessons)
            {
                if (data.Lessons.Values.Contains(e.Value<int>("id"))) continue;
                Console.WriteLine($"{e.Value<int>("id")}\t{e.Value<string>("title")}");
            }
        }
        static StepicData StepicData;
        static Structure Structure;
        static StepicSyncSettings Settings;
        static string CourseName;

        static void CreateUnits()
        {
            foreach (var section in Structure.Items.Sections().Where(z => z.Level == Settings.ModulesLevel))
            {
                var lessons = section.Items.Sections().Where(z => z.Level == Settings.LessonsLevel).ToList();
                for (int i = 0; i < lessons.Count; i++)
                {
                    var lesson = lessons[i];
                    if (StepicData.Units.Any(z => z.SectionGuid == section.Guid && z.LessonGuid == lesson.Guid)) continue;
                    var obj = new
                    {
                        lesson = StepicData.Lessons[lesson.Guid],
                        section = StepicData.Sections[section.Guid],
                        position = i + 1,
                    };

                    var r = StepicApi.Units.Create(obj);
                    var id = r.Value<int>("id");
                    StepicData.Units.Add(new StepicUnit { LessonGuid = lesson.Guid, SectionGuid = section.Guid, UnitId = id });
                    Console.WriteLine($"{section.Name} - {lesson.Name} - {id}");
                    Save();
                }
            }
        }
        static void CreateSections()
        {
            var sections = Structure.Items.Sections().Where(z => z.Level == Settings.ModulesLevel).ToList();
            for (int i = 0; i < sections.Count; i++)
            {
                var e = sections[i];
                if (StepicData.Sections.ContainsKey(e.Guid)) continue;

                var obj = new
                {
                    position = i + 1,
                    title = e.Name,
                    course = Settings.CourseNumber
                };
                var s = StepicApi.Section.Create(obj);
                StepicData.Sections[e.Guid] = s.Value<int>("id");
                Save();
                Console.WriteLine($"{e.Name} {StepicData.Sections[e.Guid]}");
            }
        }

        static void CreateLessons()
        {
            foreach (var e in Structure.Items.Sections().Where(z => z.Level == Settings.LessonsLevel))
            {
                if (StepicData.Lessons.ContainsKey(e.Guid)) continue;
                var lesson = StepicApi.Lesson.Create(new { title = e.Name });
                var id = lesson.Value<int>("id");
                StepicData.Lessons[e.Guid] = id;
                Publishing.Courses[CourseName].Save(StepicData);
                Console.WriteLine($"{e.Name} {StepicData.Lessons[e.Guid]}");
            }
        }

        static void Main(string[] args)
        {
            StepicApi.Authorize();
            CourseName = Publishing.GetCourseNameFromArgs(args);
            Settings = Publishing.Courses[CourseName].LoadInitOrEdit<CourseSettings>(3).Stepic;
            StepicData = Publishing.Courses[CourseName].LoadOrInit<StepicData>();
            Structure = Publishing.Courses[CourseName].Load<Structure>();
            //CreateLessons();
            // CreateSections();
            CreateUnits();
        }

        private static void Save()
        {
            Publishing.Courses[CourseName].Save(StepicData);
        }
    }
}
