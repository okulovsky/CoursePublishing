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

        static void CreateVideoSlide(string lessonId, int position, FileInfo file)
        {
            Console.WriteLine("Sending video " + file.FullName);
            var video = Api.SendVideo(file);
            Console.WriteLine(video.ToString());
            var step = Api.Step.Create(
                new
                {
                    block = new
                    {
                        text = "",
                        name = "video",
                        video = new
                        {
                            id = video["id"].Value<string>(),
                            status = "raw",
                            thumbnail = video["thumbnail"].Value<string>(),
                            urls = new string[] { }
                        }
                    },
                    position = 2,
                    lesson = "27378"
                });
            Console.WriteLine(step.ToString());


        }

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Api.Authorize();
            CreateVideoSlide("27378", 2, new FileInfo(@"C:\Users\Yura\Desktop\test.mp4"));
      
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
