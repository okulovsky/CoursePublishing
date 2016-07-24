using CoursePublishing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepicDeadlines
{
    class Program
    {
        static void Main(string[] args)
        {
            StepicApi.Authorize();
            var CourseName = Publishing.GetCourseNameFromArgs(args);
            var StepicData = Publishing.Courses[CourseName].Load<StepicData>();
            var Settings = Publishing.Courses[CourseName].Load<CourseSettings>().Stepic;
            var Structure = Publishing.Courses[CourseName].Load<Structure>();

            var sections = new List<int>();
            foreach (var e in Structure.Items.Sections().Where(z => z.Level == Settings.ModulesLevel))
            {
                sections.Add(StepicData.Sections[e.Guid]);
            }

            var opening = new DateTime(2016, 8, 29, 0, 0, 0);
            var softDeadlineTime = TimeSpan.FromDays(28);
            var hardDeadLineTime = TimeSpan.FromDays(42);
            var lectureTime = TimeSpan.FromDays(7);

            var currentOpening = opening;

            Func<DateTime, string> convertor = d =>
            String.Format("{0:yyyy-MM-dd}", d)
            + "T00:00:00Z";

            var position = 1;
            
            foreach (var e in sections)
            {
                var data = StepicApi.Section.GetById(e);

                     data["begin_date_source"] = convertor(currentOpening);
                     data["soft_deadline_source"] = convertor(currentOpening + softDeadlineTime);
                     data["hard_deadline_source"] = convertor(currentOpening + hardDeadLineTime);
                     data["discounting_policy"] = "first_three";
                     data["grading_policy"] = "halved";

                data.Remove("units");
                data["position"] = position++;
                data.Remove("actions");


                currentOpening += lectureTime;
                Console.WriteLine(data);
                StepicApi.Section.Update(data);
                
            }
        }
    }
}
