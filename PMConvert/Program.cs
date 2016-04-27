using CoursePublishing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuto.Model;
using Tuto.Publishing;

namespace PMConvert
{
    class Program
    {
        static Section Build(Topic topic, int level, List<VideoToTopicRelation> videoToTopic)
        {
            var section = new Section { Name = topic.Caption, Guid = topic.Guid, Level = level };
            foreach (var e in topic.Items)
                section.Sections.Add(Build(e, level + 1,videoToTopic));
            section.Videos.AddRange(videoToTopic.Where(z=>z.TopicGuid==section.Guid).OrderBy(z=>z.NumberInTopic).Select(z=>z.VideoGuid));
            return section;
        }

        static Section ProcessPMFile(string path, List<VideoToYoutubeClip> videoToYoutube, List<Video> videos)
        {


            var model = HeadedJsonFormat.Read<PublishingModel>(new FileInfo(path));
            
            videoToYoutube.AddRange(model.YoutubeClipData.Records.Select(e=>new VideoToYoutubeClip(e.Guid, e.Data.Id)));


            foreach (var z in model.Videos)
            {
                if (!videos.Any(x => x.Guid == z.Guid))
                {
                        videos.Add(new Video { Guid = z.Guid, Title = z.Name, Duration = z.Duration, OriginalLocation = z.OrdinalSuffix });
                }
            }
            return Build(model.CourseStructure.RootTopic, 0, model.CourseStructure.VideoToTopicRelations);
        }

        static void ProcessAll(string folder, params string[] files)
        {
            var videos = Publishing.LoadList<Video>();
            var vty = new List<VideoToYoutubeClip>();

            foreach(var e in files)
            {
                var section = ProcessPMFile(folder+e+".pm", vty, videos);
                Publishing.SaveCourseStructure(e, section);
            }

            Publishing.SaveList(videos);
            Publishing.SaveList(vty);

        }

        static void Main(string[] args)
        {
            ProcessAll(@"C:\Users\Yura\Desktop\TestMontage\Models\","LHPS","Hackerdom","OOP");
        }
    }
}
