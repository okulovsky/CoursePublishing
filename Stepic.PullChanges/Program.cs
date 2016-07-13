using CoursePublishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stepic.PullChanges
{
    class Program
    {
        static List<T> CheckEntity<T>(IEnumerable<T> data, StepicEntity entity, Func<T, int> stepicKeySelector, Func<T, string> titleSelector)
        {
            Console.WriteLine(entity.ToString());
            var result = new List<T>();
            foreach (var e in data)
            {

                try
                {
                    var obj = entity.GetById(stepicKeySelector(e));
                    Console.Write(".");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException?.Message.Contains("NOT FOUND") == true)
                    {

                        Console.WriteLine($"\n{titleSelector(e)} not found");
                        result.Add(e);
                    }
                    else
                        throw;
                }
            }
            return result;
        }

        static void CheckDictionary(Dictionary<Guid, int> data, StepicEntity entity)
        {
            var delete = CheckEntity(data, entity, z => z.Value, z => z.Key.ToString());
            foreach (var e in delete)
                data.Remove(e.Key);
        }

        static void CheckList<T>(List<T> data, StepicEntity entity, Func<T, int> stepicKeySelector, Func<T, string> titleSelector)
        {
            var delete = CheckEntity(data, entity, stepicKeySelector, titleSelector);
            foreach (var e in delete)
                data.Remove(e);
        }


        const string CourseName = "LHPS";


        static void Main(string[] args)
        {
            var data = Publishing.Courses[CourseName].Load<StepicData>();
            StepicApi.Authorize();
            CheckDictionary(data.Lessons, StepicApi.Lesson);
            CheckDictionary(data.Steps, StepicApi.Step);
            CheckDictionary(data.Sections, StepicApi.Section);
            CheckList(data.Units, StepicApi.Units, z => z.UnitId, z => z.SectionGuid + "->" + z.LessonGuid);
            Publishing.Courses[CourseName].Save(data);
        }
    }
}
