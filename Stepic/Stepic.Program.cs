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
            var video = Api.SendVideo(file,lessonId);
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
                    position = position,
                    lesson = lessonId
                });
            Console.WriteLine(step.ToString());


        }

        static void Main()
        {
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Api.Authorize();
            CreateVideoSlide("27378",3, new FileInfo(@"C:\Users\Yura\Desktop\test1.mp4"));
      
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
