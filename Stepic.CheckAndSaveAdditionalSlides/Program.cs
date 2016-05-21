using CoursePublishing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stepic.CheckAndSaveAdditionalSlides
{
    class LessonDump
    {
        public int Id;
        public string Title;
        public List<JObject> Steps;
    }


    class Program
    {
        static void LoadDump(StepicSyncSettings Settings, List<int> lessonIndices, string CourseName)
        {
            Console.WriteLine("Getting lessons...");
            var lessons = StepicApi.Lesson
                .Get(new { teacher = Settings.TeacherId })
                .Select(z => new LessonDump
                {
                    Id = z["id"].Value<int>(),
                    Title = z["title"].Value<string>()
                })
                .OrderBy(z => lessonIndices.IndexOf(z.Id))
                .ToList();

            foreach (var e in lessons)
            {
                e.Steps = StepicApi.Step
                    .Get(new { lesson = e.Id })
                    .OrderBy(z => z["position"].Value<int>())
                    .ToList();
            }
            Publishing.Courses[CourseName].SaveList(lessons);
        }


        static void Main(string[] args)
        {
            StepicApi.Authorize();
            var CourseName = Publishing.GetCourseNameFromArgs(args);
            var StepicData = Publishing.Courses[CourseName].Load<StepicData>();
            var Settings = Publishing.Courses[CourseName].Load<CourseSettings>().Stepic;
            var Structure = Publishing.Courses[CourseName].Load<Structure>();

            var videoTitles = (
                from guid in Structure.Items.VideoGuids()
                join video in Publishing.Common.LoadList<Video>() on guid equals video.Guid
                select new { guid, video }
                )
                .ToDictionary(z => StepicData.Steps[z.guid], z => z.video.Title);

            var lessonIndices = Structure.Items
                .Sections()
                .Where(z => z.Level == Settings.LessonsLevel)
                .Select(z => StepicData.Lessons[z.Guid])
                .ToList();

            //LoadDump();
            var lessons = Publishing.Courses[CourseName].LoadList<LessonDump>();


            var builder = new StringBuilder();
            foreach(var e in lessons)
            {
                builder.AppendLine($"<h1>{e.Title}</h1>");
                builder.AppendLine();
                builder.AppendLine();
                bool lastIsVideo = false;
                for (int i=0;i<e.Steps.Count;i++)
                {
                    var s = e.Steps[i];
                    if (s["block"]["name"].Value<string>()=="video")
                    {
                        if (lastIsVideo)
                        {
                            builder.AppendLine("<p><b>Missing exercise</b></p>");
                        }
                        var id = s["id"].Value<int>();
                        builder.AppendLine($"<a href=\"https://stepic.org/edit-lesson/{e.Id}/step/{s["position"]}\"><h2>{videoTitles[id]}</h2></a>");
                        builder.AppendLine();
                        lastIsVideo = true;
                    }
                    else
                    {
                        lastIsVideo = false;
                        builder.AppendLine(s["block"]["text"].Value<string>());
                        builder.AppendLine();
                    }
                }
            }
            File.WriteAllText(
                Publishing.Courses[CourseName].GetFileName("Reminder.html"),
                $"<html><head><meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\"/></head><body>{builder.ToString()}</body></html>"
                );
        }
    }
}
