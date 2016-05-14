using CoursePublishing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldProjectConversion
{
    class Program
    {
        static JToken OpenOldFormat(string directory, string path)
        {
            var text = File.ReadLines(Path.Combine(directory,path)).Skip(1).Aggregate((a, b) => a + "\n" + b);
            return JToken.Parse(text);
        }

        static Section Process(bool root, JObject oldInfo, Dictionary<Guid,Section> index, int level)
        {
            Section s = new Section();
            if (root) s = new Structure();
            s.Guid = Guid.Parse(oldInfo["Guid"].Value<string>());
            s.Name = oldInfo["Caption"].Value<string>();
            s.Level = level;
            index[s.Guid] = s;
            foreach(var e in (oldInfo["Items"] as JArray))
            {
                s.Sections.Add(Process(false, e as JObject, index, level+1));
            }
            for (int i = 0; i < s.Sections.Count; i++)
                s.Sections[i].Order = i;
            return s;
        }

        static void ProcessFolder(string directory, string CourseName)
        {
            var oldStr = OpenOldFormat(directory, "CourseStructure.txt");
            Console.WriteLine(oldStr.ToString().Substring(0, 5000));
            var root = oldStr["RootTopic"];
            var index=new Dictionary<Guid,Section>();
            var str = Process(true, root as JObject, index,0);

            var rel1 = (oldStr["VideoToTopicRelations"] as JArray)
                .Select(z => new {
                    VideoGuid = Guid.Parse(z["VideoGuid"].Value<string>()),
                    TopicGuid = Guid.Parse(z["TopicGuid"].Value<string>()),
                    N = z["NumberInTopic"].Value<int>() })
                .ToList();

            var rel=rel1
                .GroupBy(z => z.TopicGuid)
                .Select(z => new { Topic = z.Key, Items = z.OrderBy(x => x.N).Select(x => x.VideoGuid).ToList() })
                .ToList();

            foreach(var e in rel)
            {
                foreach (var z in e.Items)
                    if (index.ContainsKey(e.Topic))
                        index[e.Topic].Videos.Add(z);
            }


            Publishing.SaveCourseStructure(str as Structure, CourseName);


            var videos = OpenOldFormat(directory, "VideoSummaries.txt");
            Console.WriteLine(videos.ToString());
            var result1 = (videos as JArray)
                .Select(z => new Video
                {
                    Guid = Guid.Parse(z["Guid"].Value<string>()),
                    Title = z["Name"].Value<string>(),
                    OriginalLocation = CourseName + "_fromBackup",
                    EpisodeNumber = 0,
                    Duration = TimeSpan.FromSeconds(1)
                })
                .ToList();
            Publishing.Common.UpdateList<Video>(result1, z => z.Guid.ToString());

            var result2 =
                (OpenOldFormat(directory, "YoutubeClip.layer.txt")["Records"] as JArray)
                .Select(z =>
                    new VideoToYoutubeClip(
                        Guid.Parse(z["Guid"].Value<string>()),
                        z["Data"]["Id"].Value<string>()))
                .ToList();
            Publishing.Common.UpdateList<VideoToYoutubeClip>(result2, z => z.Guid.ToString());

            var result3 =
               (OpenOldFormat(directory, "YoutubePlaylist.layer.txt")["Records"] as JArray)
               .Select(z =>
                   new TopicToYoutubePlaylist(
                       Guid.Parse(z["Guid"].Value<string>()),
                       z["Data"]["PlaylistId"].Value<string>()))
               .ToList();
            Publishing.Common.UpdateList<TopicToYoutubePlaylist>(result3, z => z.Guid.ToString());
        }


        static void Main(string[] args)
        {
            //ProcessFolder(@"C:\Users\Yura\Desktop\OldPublishings\AIML\Publishing","AIML");
            //ProcessFolder(@"C:\Users\Yura\Desktop\OldPublishings\BP-Publishing\volume1\", "BP1");
            ProcessFolder(@"C:\Users\Yura\Desktop\OldPublishings\BP-Publishing\volume2\", "BP2");

        }
    }
}
