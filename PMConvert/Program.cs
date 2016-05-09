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
        static Section Build(int order, Topic topic, int level, List<VideoToTopicRelation> videoToTopic)
        {
            var section = new Section { Name = topic.Caption, Guid = topic.Guid, Level = level, Order=order };
            for (int i=0;i<topic.Items.Count;i++)
                section.Sections.Add(Build(i,topic.Items[i], level + 1,videoToTopic));
            section.Videos.AddRange(videoToTopic.Where(z=>z.TopicGuid==section.Guid).OrderBy(z=>z.NumberInTopic).Select(z=>z.VideoGuid));
            return section;
        }

        static Section ProcessPMFile(string path, List<VideoToYoutubeClip> videoToYoutube, List<Video> videos, List<TopicToYoutubePlaylist> topicToYoutube)
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

            foreach(var x in model.YoutubePlaylistData.Records)
            {
                topicToYoutube.Add(new TopicToYoutubePlaylist(x.Guid, x.Data.PlaylistId));
            }


            return Build(0,model.CourseStructure.RootTopic, 0, model.CourseStructure.VideoToTopicRelations);
        }

        static void ProcessAll(string folder, params string[] files)
        {
            var videos = Publishing.LoadList<Video>();
            var vty = new List<VideoToYoutubeClip>();
            var tty = new List<TopicToYoutubePlaylist>();

            foreach(var e in files)
            {
                var section = ProcessPMFile(folder+e+".pm", vty, videos,tty);
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
