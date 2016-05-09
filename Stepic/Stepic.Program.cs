using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl;
using CoursePublishing;
using System.IO;
using Newtonsoft.Json.Linq;
namespace Stepic
{

    class Program
    {
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Api.Authorize();
          //  var lesson = Api.Lesson.Create(new { title = "TestTestTest" });
            var step = Api.Step.Create(
                new
                {
                    has_instruction = false,
                    cost = 0,
                    block = new
                    {
                        text = "<p>Hello world!</p>",
                        name = "text",
                    },
                    position = 2,
                    lesson = "27378"
                });
            Console.WriteLine("Check if everithing ready");
            Console.ReadKey();
            Api.Step.Delete(step["id"]);
          //  Api.Lesson.Delete(lesson["id"]);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs _e)
        {
            Exception e = (Exception)_e.ExceptionObject;
            while(e!=null)
            {
                Console.WriteLine(e.Message);
                e = e.InnerException;
            }
            Console.ReadKey();
            Environment.Exit(0);

        }
    }
}
