using CoursePublishing;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeCourseSync
{
    class Program
    {
        static string CourseName;
        static Section Structure;
        static Dictionary<Guid,string> Relation;
        static Dictionary<string, YoutubeClip> Clips;
        static Dictionary<Guid,Video> Videos;
        static int[] Margins;
        static YoutubeSyncSettings Settings;
        static YouTubeService Service;


        static bool CheckMissingVideos()
        {
            var videoGuids = Structure.Items.VideoGuids().ToList();
            var missingVideos = videoGuids.Where(z => !Relation.ContainsKey(z)).ToList();
            if (missingVideos.Count != 0)
            {
                Console.WriteLine("Some videos are missing from youtube. Unable to proceed. Sync with youtube or correct the course structure. Missing videos are:");
                foreach (var e in missingVideos)
                {
                    Console.WriteLine(Videos
                        .Values
                        .Where(z => z.Guid == e)
                        .Select(z => z.Title + " (" + z.Guid + ")")
                        .FirstOrDefault());
                }
                return false;
            }
            return true;
        }

        static void MakeMargins()
        {
            Margins = Structure
                .ItemsWithPathes
                .Select(z=>z.Item1)
                .SelectMany(z=>Enumerable.Range(0,z.Count).Select(x=>new { Level=x, Index=z[x].Item2 }))
                .GroupBy(z => z.Level)
                .OrderBy(z => z.Key)
                .Select(x => x.Max(z => z.Index).ToString().Length)
                .ToArray();
        }

        static string CreatePrefix(List<Tuple<Section,int>> list)
        {
            return Enumerable
               .Range(0, list.Count)
               .Select(z => string.Format("{0:D" + Margins[z] + "}", list[z].Item2+1))
               .Aggregate((a, b) => a + "-" + b);
        }



        static void UpdateVideos()
        {
            foreach (var data in Structure.ItemsWithPathes.Where(z => z.Item2.VideoGuid != null))
            {
                var video = Videos[data.Item2.VideoGuid.Value];
                var title = CourseName + "-" + CreatePrefix(data.Item1) + " " + video.Title;
                var description = "";

                if (!Relation.ContainsKey(video.Guid)) continue;
                var youtubeClip = Relation[video.Guid];
                if (Clips.ContainsKey(youtubeClip))
                {
                    var clip = Clips[youtubeClip];
                    if (clip.Name == title && clip.Description == description)
                        continue;
                }

                Console.Write($"Updating {title}...");
                var listRq = Service.Videos.List("snippet");
                listRq.Id = youtubeClip;
                var remoteClips = listRq.Execute();
                if (remoteClips.Items.Count == 0)
                {
                    Console.WriteLine("Not found at youtube");
                    continue;
                }
                var remoteClip=remoteClips.Items[0];
                remoteClip.Snippet.Title = title;
                remoteClip.Snippet.Description = description;
                remoteClip.Snippet.Tags = null;
                Service.Videos.Update(remoteClip, "snippet").Execute();
                Clips[youtubeClip] = new YoutubeClip
                {
                    Id = youtubeClip,
                    Name = title,
                    Description = description
                };
                Console.WriteLine("Done");
            }
        }
        

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Pass the name of the course as the first argument");
                return;
            }
            CourseName = args[0];

            Service = Publishing.InitializeYoutube();
            Settings = Publishing.LoadOrInit<YoutubeSyncSettings>(CourseName);

            Structure = Publishing.LoadCourseStructure(CourseName);
            Relation = Publishing.LoadList<VideoToYoutubeClip>().ToDictionary(z => z.Guid, z => z.YoutubeId);

  

            Videos = Publishing.LoadList<Video>().ToDictionary(z=>z.Guid,z=>z);
            Clips = Publishing.LoadList<YoutubeClip>().ToDictionary(z => z.Id, z => z);

            // if (!CheckMissingVideos()) return;

            MakeMargins();
            UpdateVideos();



            Publishing.SaveList(Clips.Values.ToList());

            
        }
    }
}
