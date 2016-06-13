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
                .GetByRequestString(new { teacher = Settings.TeacherId })
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
                    .GetByRequestString(new { lesson = e.Id })
                    .OrderBy(z => z["position"].Value<int>())
                    .ToList();
            }
            Publishing.Courses[CourseName].SaveList(lessons);
        }


        static bool IsVideo(JObject obj)
        {
            return obj["block"]["name"].Value<string>() == "video";
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

            LoadDump(Settings,lessonIndices,CourseName);

            var lessons = Publishing.Courses[CourseName].LoadList<LessonDump>();


            var builder = new StringBuilder();
            var videoCount = 0;
            var nonVideoCount = 0;
            foreach (var e in lessons)
            {
                builder.AppendLine($"<h1>{e.Title}</h1>");
                builder.AppendLine();
                builder.AppendLine();
                
               for (int i=0;i<e.Steps.Count;i++)
                {
                    var s = e.Steps[i];
                    if (IsVideo(s))
                    {
                        var id = s["id"].Value<int>();
                        if (i == e.Steps.Count - 1 || IsVideo(e.Steps[i + 1]))
                        {
                            builder.AppendLine($"<h2><a href=\"https://stepic.org/edit-lesson/{e.Id}/step/{s["position"]}\">[Add]</a> <font color=brown>{videoTitles[id]}</font></h2>");
                        }
                        else
                        {
                            builder.AppendLine($"<h2>{videoTitles[id]}</h2></a>");
                        }
                        videoCount++;
                    }
                    else
                    {
                        builder.AppendLine(s["block"]["text"].Value<string>());
                        builder.Append($"<a href=\"https://stepic.org/edit-lesson/{e.Id}/step/{s["position"]}\">[Edit]</a>");
                        builder.AppendLine();
                        nonVideoCount++;
                    }
                }
            }
            File.WriteAllText(
                Publishing.Courses[CourseName].GetFileName("Reminder.html"),
                $"<html><head><meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\"/></head><body>{builder.ToString()}</body></html>"
                );
            Console.WriteLine($"Non-video percentage {nonVideoCount*100.0/(nonVideoCount+videoCount)}");
        }
    }
}
