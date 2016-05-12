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
    class YoutubeCourseSyncService : PreviewableService
    {
        string CourseName;
        Section Structure;
        Dictionary<Guid, YoutubeClip> Clips;
        Dictionary<Guid, CoursePublishing.Video> Videos;
        Dictionary<Guid, YoutubePlaylist> Playlists;
        int[] Margins;
        YoutubeSyncSettings Settings;
        YouTubeService Service;


        static void Main(string[] args)
        {
            new YoutubeCourseSyncService().Run(args);
        }

        void Run(string[] args)
        {
            CourseName = Publishing.GetCourseNameFromArgs(args);
            Initialize();
            if (!CheckMissingVideos()) return;
            MakeMargins();
            UpdateVideos();
            foreach (var e in Settings.PlayListLevels)
                UpdatePlaylistsForLevel(e);

            if (AskAndExecute())
            {
                Publishing.Common.UpdateList(Clips.Values.ToList(), z => z.Id);
                Publishing.Common.UpdateList(Playlists.Values.ToList(), z => z.Id);
                Publishing.Common.UpdateList(
                    Playlists.Select(z => new TopicToYoutubePlaylist(z.Key, z.Value.Id)).ToList(),
                    z => z.Guid.ToString());
            }
        }

        void Initialize()
        {
            
            Settings = Publishing.Courses[CourseName].LoadInitOrEdit<CourseSettings>(5).Youtube;
            Service = Publishing.InitializeYoutube(Settings.Channel);
            Structure = Publishing.Courses[CourseName].Load<Structure>(); ;


            Clips =
                (from guid in Structure.Items.VideoGuids()
                 join rel in Publishing.Common.LoadList<VideoToYoutubeClip>() on guid equals rel.Guid
                 join clip in Publishing.Common.LoadList<YoutubeClip>() on rel.YoutubeId equals clip.Id
                 select new { guid, clip }
                ).ToDictionary(z => z.guid, z => z.clip);

            Playlists =
                (from section in Structure.Items.Sections()
                 join rel in Publishing.Common.LoadList<TopicToYoutubePlaylist>() on section.Guid equals rel.Guid
                 join list in Publishing.Common.LoadList<YoutubePlaylist>() on rel.YoutubeId equals list.Id
                 select new { section, list }
                 ).ToDictionary(z => z.section.Guid, z => z.list);


            Videos =
                (from guid in Structure.Items.VideoGuids()
                 join video in Publishing.Common.LoadList<CoursePublishing.Video>() on guid equals video.Guid
                 select new { guid, video }
                 ).ToDictionary(z => z.guid, z => z.video);


        }

        bool CheckMissingVideos()
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
        void MakeMargins()
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

        string CreatePrefix(List<SectionIndexation> list)
        {
            return Enumerable
               .Range(0, list.Count)
               .Select(z => string.Format("{0:D" + Margins[z] + "}", list[z].Index+1))
               .Aggregate((a, b) => a + "-" + b);
        }

        string CreateTitle(List<SectionIndexation> list,string title)
        {
            return $"{CourseName}-{CreatePrefix(list)} {title}".Trim();
        }
        #endregion

        #region Updating videos
        void UpdateVideos()
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

                AddAction($"Updating video {title}",
                    () =>
                    {
                        var listRq = Service.Videos.List("snippet");
                        listRq.Id = clip.Id;
                        var remoteClips = listRq.Execute();
                        if (remoteClips.Items.Count == 0)
                            throw new Exception("Not found at Youtube");
                        var remoteClip = remoteClips.Items[0];
                        remoteClip.Snippet.Title = title;
                        remoteClip.Snippet.Description = description;
                        remoteClip.Snippet.Tags = null;
                        Service.Videos.Update(remoteClip, "snippet").Execute();
                    });
            }
        }
        #endregion

        void UpdatePlaylistsForLevel(int level)
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
                var videos = topic.Items.VideoGuids().Where(z => Clips.ContainsKey(z)).Select(z => Clips[z].Id).ToList();

                if (!Playlists.ContainsKey(topic.Guid))
                {
                    AddAction($"Creating playlist {title}", () =>
                     {
                         var pl = Playlists[topic.Guid] = CreatePlaylist(title);
                         UpdateEntries(pl, videos);
                     });
                    continue;
                }
                var list = Playlists[topic.Guid];
                if (list.Title != title)
                {
                    AddAction($"Updating playlit {title}", ()=> UpdateListTitle(list, title));
                }

                bool wrongEntires =
                    list.Entries.Count!=videos.Count ||
                    list.Entries.Select(z => z.VideoId).Zip(videos, (s1, s2) => s1 == s2).Any(z => !z);
                if (wrongEntires)
                {
                    AddAction($"Updating playlist enties for {title}", () => UpdateEntries(list, videos));
                }
            }

        }

        private void UpdateEntries(YoutubePlaylist list,List<string> newEntries)
        {
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
        }

        private void UpdateListTitle(YoutubePlaylist list,string title)
        {
            list.Title = title;
            var listRq = Service.Playlists.List("snippet");
            listRq.Id = list.Id;
            var lists = listRq.Execute();
            if (lists.Items.Count==0)
            {
                throw new Exception("List not found at youtube");
            }
            var pl=lists.Items[0];
            pl.Snippet.Title = list.Title;
            Service.Playlists.Update(pl, "snippet");
        }

        private YoutubePlaylist CreatePlaylist(string title)
        {
            var playlist = new YoutubePlaylist();
            playlist.Id = "";
            playlist.Title = title;
            var list = new Playlist();
            list.Snippet = new PlaylistSnippet();
            list.Snippet.Title = title;
            list.Status = new PlaylistStatus();
            list.Status.PrivacyStatus = "public";
            list = Service.Playlists.Insert(list, "snippet,status").Execute();
            playlist.Id = list.Id;
            return playlist;
        }
    }
}
