using CoursePublishing;
using System;
using System.Collections.Generic;
using System.IO;
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
        static Dictionary<Guid, Video> Videos;
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

        static void UploadVideoSlide(Section section, Video video)
        {
            if (!StepicData.Lessons.ContainsKey(section.Guid))
            {
                Console.WriteLine($"Lesson {section.Name} is missing at Stepic");
                return;
            }
            if (StepicData.Videos.ContainsKey(video.Guid))
            {
                return;
            }
            var file = new DirectoryInfo(Settings.TutoOutputFolder)
                .GetFiles("*.mp4", SearchOption.TopDirectoryOnly)
                .Where(z => z.Name.Contains(video.Guid.ToString()))
                .FirstOrDefault();
            if (file==null)
            {
                Console.WriteLine($"Can't find video file for video {video.Title}, GUID={video.Guid}");
                return;
            }
            Console.WriteLine($"Uploading video {video.Title}");
            var uploadedVideo = StepicApi.SendVideo(file, StepicData.Lessons[section.Guid].ToString());
            StepicData.Videos[video.Guid] = int.Parse(uploadedVideo.Value<string>("id"));
            StepicData.Thumbnails[video.Guid] = uploadedVideo.Value<string>("thumbnail");
            Console.WriteLine("Video uploaded");
            Save();
        }

        static void CreateVideoStep(Section section, Video video)
        {
            if (!StepicData.Videos.ContainsKey(video.Guid))
            {
                Console.WriteLine($"Video {video.Title} was not uploaded");
                return;
            }
            var position = section.Items.VideoGuids().ToList().IndexOf(video.Guid) + 1;

            Console.WriteLine($"Creating step {video.Title}...");
            var step = StepicApi.Step.Create(
               new
               {
                   block = new
                   {
                       text = "",
                       name = "video",
                       video = new
                       {
                           id = StepicData.Videos[video.Guid].ToString(),
                           status = "raw",
                           thumbnail = StepicData.Thumbnails[video.Guid].ToString(),
                           urls = new string[] { }
                       }
                   },
                   position = position,
                   lesson = StepicData.Lessons[section.Guid]
               });
            StepicData.Steps[video.Guid] = int.Parse(step.Value<string>("id"));
            Save();
            Console.WriteLine("Done");
        }

        static void UploadAllVideo()
        {
            foreach(var s in Structure.Items.Sections().Where(z=>z.Level==Settings.LessonsLevel))
                foreach(var v in s.Items.VideoGuids())
                {
                    UploadVideoSlide(s, Videos[v]);
                    CreateVideoStep(s, Videos[v]);
                    return;
                }
        }

        static void Main(string[] args)
        {
            StepicApi.Authorize();
            CourseName = Publishing.GetCourseNameFromArgs(args);
            Settings = Publishing.Courses[CourseName].LoadInitOrEdit<CourseSettings>(3).Stepic;
            StepicData = Publishing.Courses[CourseName].LoadOrInit<StepicData>();
            Structure = Publishing.Courses[CourseName].Load<Structure>();
            Videos = (
                from guid in Structure.Items.VideoGuids()
                join video in Publishing.Common.LoadList<Video>() on guid equals video.Guid
                select new { guid, video }
                ).ToDictionary(z => z.guid, z => z.video);

            //CreateLessons();
            // CreateSections();
            // CreateUnits();
            UploadAllVideo();
        }

        private static void Save()
        {
            Publishing.Courses[CourseName].Save(StepicData);
        }
    }
}
