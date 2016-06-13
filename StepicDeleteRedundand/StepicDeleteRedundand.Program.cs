using CoursePublishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepicDeleteRedundand
{
    static class EE
    {
        public static IEnumerable<Tuple<int,T>> WithIndex<T>(this IEnumerable<T> en)
        {
            int index = 0;
            foreach(var e in en)
            {
                yield return Tuple.Create(index, e);
                index++;
            }   
        }
    }


    class Delete  : CoursePublishing.PreviewableService
    {
        static void Main(string[] args)
        {
            new Delete().Run(args);
        }


        void Filter(Dictionary<int,string> existed, IEnumerable<int> stored, StepicEntity entity)
        {
            foreach (var e in existed)
                if (!stored.Contains(e.Key))
                    AddAction("Deleting " + e.Value, () => entity.Delete(e.Key.ToString()));
        }

        IEnumerable<Tuple<int,string>> GetStepsForLesson(int id, string title)
        {
            var stepsInLesson =
                     StepicApi.Step.GetByRequestString(new { lesson = id });

            var ss = stepsInLesson
                    .WithIndex()
                    .Select(x => Tuple.Create(
                        x.Item2.Value<int>("id"),
                        title + "/step" + x.Item1
                    ))
                    .ToList();
            return ss;
        }

        void Run(string[] args)
        {
            StepicApi.Authorize();
            var CourseName = Publishing.GetCourseNameFromArgs(args);
            var StepicData = Publishing.Courses[CourseName].Load<StepicData>();
            var Settings = Publishing.Courses[CourseName].Load<CourseSettings>().Stepic;



            Console.WriteLine("Getting lessons...");
            var lessons = StepicApi.Lesson
                .GetByRequestString(new { teacher = Settings.TeacherId })
                .ToDictionary(z => z.Value<int>("id"), z => z.Value<string>("title"));


     

            Dictionary<int, string> steps = new Dictionary<int, string>();
            foreach(var e in lessons)
            {
                Console.WriteLine($"Getting steps for lesson {e.Key}/{e.Value}");
                
                foreach (var x in GetStepsForLesson(e.Key,e.Value))
                    steps[x.Item1] = x.Item2;
            }

          

            Filter(steps, StepicData.Steps.Values, StepicApi.Step);
            Filter(lessons, StepicData.Lessons.Values, StepicApi.Lesson);
            AskAndExecute();
        }
    }
}
