using CoursePublishing;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
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
        static Dictionary<Guid, YoutubeClip> Clips;
        static Dictionary<Guid, CoursePublishing.Video> Videos;
        static Dictionary<Guid, YoutubePlaylist> Playlists;
        static int[] Margins;
        static YoutubeSyncSettings Settings;
        static YouTubeService Service;
        static bool Preview;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Pass the name of the course as the first argument");
                return;
            }

            CourseName = args[0];

            if (args.Length > 1)
            {
                Preview = args[1] == "Preview";
            }

            Settings = Publishing.Courses[CourseName].LoadInitOrEdit<CourseSettings>(5).Youtube;


            Service = Publishing.InitializeYoutube();

            Structure = Publishing.Courses[CourseName].Load<Structure>(); ;


            Clips =
                (from rel in Publishing.Common.LoadList<VideoToYoutubeClip>()
                 join clip in Publishing.Common.LoadList<YoutubeClip>() on rel.YoutubeId equals clip.Id
                 select new { rel, clip }
                ).ToDictionary(z => z.rel.Guid, z => z.clip);

            Playlists =
                (from rel in Publishing.Common.LoadList<TopicToYoutubePlaylist>()
                 join list in Publishing.Common.LoadList<YoutubePlaylist>() on rel.YoutubeId equals list.Id
                 select new { rel, list }
                 ).ToDictionary(z => z.rel.Guid, z => z.list);



            Videos = Publishing.Common.LoadList<CoursePublishing.Video>().ToDictionary(z => z.Guid, z => z);


            if (!CheckMissingVideos()) return;

            MakeMargins();
            UpdateVideos();
            foreach (var e in Settings.PlayListLevels)
                UpdatePlaylistsForLevel(e);

            if (!Preview)
            {
                Publishing.Common.UpdateList(Clips.Values.ToList(), z => z.Id);
                Publishing.Common.UpdateList(Playlists.Values.ToList(), z => z.Id);
                Publishing.Common.UpdateList(
                    Playlists.Select(z => new TopicToYoutubePlaylist(z.Key, z.Value.Id)).ToList(),
                    z => z.Guid.ToString());
            }
        }


        static bool CheckMissingVideos()
        {
            var videoGuids = Structure.Items.VideoGuids().ToList();
            var missingVideos = videoGuids.Where(z => !Clips.ContainsKey(z)).ToList();
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

        #region Margins and Prefixes
        static void MakeMargins()
        {
            Margins = Structure
                .ItemsWithPathes
                .Select(z=>z.Path)
                .SelectMany(z=>Enumerable.Range(0,z.Count).Select(x=>new { Level=x, Index=z[x].Index }))
                .GroupBy(z => z.Level)
                .OrderBy(z => z.Key)
                .Select(x => x.Max(z => z.Index).ToString().Length)
                .ToArray();
        }

        static string CreatePrefix(List<SectionIndexation> list)
        {
            return Enumerable
               .Range(0, list.Count)
               .Select(z => string.Format("{0:D" + Margins[z] + "}", list[z].Index+1))
               .Aggregate((a, b) => a + "-" + b);
        }

        static string CreateTitle(List<SectionIndexation> list,string title)
        {
            return $"{CourseName}-{CreatePrefix(list)} {title}".Trim();
        }
        #endregion

        #region Updating videos
        static void UpdateVideos()
        {
            foreach (var data in Structure.ItemsWithPathes.Where(z => z.Item.VideoGuid != null))
            {
                var video = Videos[data.Item.VideoGuid.Value];
                var title = CreateTitle(data.Path, video.Title);
                var description = "";


                if (!Clips.ContainsKey(video.Guid)) continue;

                var clip = Clips[video.Guid];
                if (clip.Name == title && clip.Description == description)
                        continue;

                clip.Name = title;
                clip.Description = description;

                Console.Write($"Updating video {title}...");
                if (Preview)
                {
                    Console.WriteLine("Previewed");
                    continue;
                }


                var listRq = Service.Videos.List("snippet");
                listRq.Id = clip.Id;
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

                Console.WriteLine("Done");
            }
        }
        #endregion

        static void UpdatePlaylistsForLevel(int level)
        {

            var test = Structure
                .ItemsWithPathes
                .ToList();

            var inList=test
                .Where(z => z.Item.Section != null)
                .Where(z => z.Item.Section.Level == level)
                .ToList();
            foreach(var data in inList)
            {
                var title = CreateTitle(data.Path,data.Item.Section.Name);

                var topic = data.Item.Section;

                if (!Playlists.ContainsKey(topic.Guid))
                {
                    Playlists[topic.Guid] = CreatePlaylist(title);
                }
                var list = Playlists[topic.Guid];

                var videos = topic.Items.VideoGuids().Where(z => Clips.ContainsKey(z)).Select(z => Clips[z].Id).ToList();


                if (list.Title != title)
                {
                    UpdateListTitle(list, title);
                }

                bool wrongEntires =
                    list.Entries.Count!=videos.Count ||
                    list.Entries.Select(z => z.VideoId).Zip(videos, (s1, s2) => s1 == s2).Any(z => !z);
                if (wrongEntires)
                {
                    UpdateEntries(list,videos);
                }
            }

        }

        private static void UpdateEntries(YoutubePlaylist list,List<string> newEntries)
        {
            Console.Write($"Updating playlist {list.Title}...");

            if (Preview)
            {
                Console.WriteLine("Previewed");
                return;
            }


            foreach (var e in list.Entries)
                Service.PlaylistItems.Delete(e.Id).Execute();
            list.Entries.Clear();

            foreach (var e in newEntries)
            {
                var item = new PlaylistItem
                {
                    Snippet = new PlaylistItemSnippet
                    {
                        PlaylistId = list.Id,
                        ResourceId = new ResourceId
                        {
                            Kind = "youtube#video",
                            VideoId = e
                        }
                    }
                };
                var result=Service.PlaylistItems.Insert(item, "snippet").Execute();
                list.Entries.Add(new YoutubePlaylistEntry { Id = result.Id, VideoId = e });
            }
            Console.WriteLine("Done");
        }

        private static void UpdateListTitle(YoutubePlaylist list,string title)
        {
            list.Title = title;
            Console.Write($"Update playlist {list.Title}... ");
            if (Preview)
            {
                Console.WriteLine("Previewed");
                return;
            }

            var listRq = Service.Playlists.List("snippet");
            listRq.Id = list.Id;
            var lists = listRq.Execute();
            if (lists.Items.Count==0)
            {
                Console.WriteLine("Failed");
                return;
            }
            var pl=lists.Items[0];
            pl.Snippet.Title = list.Title;
            Service.Playlists.Update(pl, "snippet");
            Console.WriteLine("Done");
        }

        private static YoutubePlaylist CreatePlaylist(string title)
        {
            Console.Write($"Creating playlist {title}... ");
            var playlist = new YoutubePlaylist();
            playlist.Id = "";
            playlist.Title = title;

            if (Preview)
            {
                Console.WriteLine("Previewed");
                return playlist;
            }


            var list = new Playlist();
            list.Snippet = new PlaylistSnippet();
            list.Snippet.Title = title;
            list.Status = new PlaylistStatus();
            list.Status.PrivacyStatus = "public";
            list = Service.Playlists.Insert(list, "snippet,status").Execute();
            playlist.Id = list.Id;
            Console.WriteLine("Done");
            return playlist;
        }


    }
}
