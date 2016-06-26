using CoursePublishing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateVideoDescriptionFromOldFormat
{
    class Program
    {
        static void Main(string[] args)
        {
            var directoryName = @"C:\Users\Yura\Desktop\testing-lecture01";

            var directory = new DirectoryInfo(directoryName);
            var files = directory.GetFiles("local.tuto", SearchOption.AllDirectories);
            var newVideos = new List<Video>();
            foreach (var e in files)
            {
                var text = File.ReadLines(e.FullName).Skip(1).Aggregate((a, b) => a + "\n" + b);
                var obj = JObject.Parse(text);
                var info = obj["MontageModel"]["Information"]["Episodes"] as JArray;
                var number = 0;
                foreach(var z in info)
                {
                    Video v = new Video
                    {
                        Duration = TimeSpan.FromSeconds(0),
                         Guid=Guid.Parse(z["Guid"].Value<string>()),
                          EpisodeNumber=number++,
                           OriginalLocation=directory.Name+"-"+e.Directory.Name,
                            Title=z["Name"]  .Value<string>() 
                    };
                    newVideos.Add(v);

                    Console.WriteLine(JObject.FromObject(v));
                }
           }

            Publishing.Common.UpdateList(newVideos, z => z.Guid.ToString());

        }
    }
}
