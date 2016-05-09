using CoursePublishing;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace YoutubeSync
{
    class YoutubeSync
    {
        static YouTubeService service;



        static List<YoutubeClip> GetAllClips()
        {
            Console.Write("Retrieving video data");

            var videos = new List<YoutubeClip>();
            var channelsListRequest = service.Channels.List("contentDetails");
            channelsListRequest.Mine = true;
            

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = channelsListRequest.Execute();

            var ids = new List<string>();

            foreach (var channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                var nextPageToken = "";
                while (nextPageToken != null)
                {
                    var playlistItemsListRequest = service.PlaylistItems.List("snippet");
                    playlistItemsListRequest.PlaylistId = uploadsListId;
                    playlistItemsListRequest.MaxResults = 50;
                    playlistItemsListRequest.PageToken = nextPageToken;

                    // Retrieve the list of videos uploaded to the authenticated user's channel.
                    var playlistItemsListResponse = playlistItemsListRequest.Execute();


                    foreach (var playlistItem in playlistItemsListResponse.Items)
                    {
                        var snippet = playlistItem.Snippet;
                        ids.Add(snippet.ResourceId.VideoId);
                        videos.Add(new YoutubeClip { Id = snippet.ResourceId.VideoId, Name = snippet.Title, Description = snippet.Description });
                    }
                    Console.Write(".");
                    nextPageToken = playlistItemsListResponse.NextPageToken;
                }
            }
            Console.WriteLine();
            return videos;
        }

        private static void UpdateAllPlaylistItems(YoutubePlaylist list)
        {
            var nextToken = "";
            while (nextToken != null)
            {
                var itemsRq = service.PlaylistItems.List("snippet,contentDetails");
                itemsRq.PageToken = nextToken;
                itemsRq.PlaylistId = list.Id;

                var items = itemsRq.Execute();

                foreach (var e in items.Items)
                    list.Entries.Add(new YoutubePlaylistEntry { Id = e.Id, VideoId = e.ContentDetails.VideoId });

                nextToken = items.NextPageToken;
            }
        }

        public static List<YoutubePlaylist> GetAllPlaylists()
        {
            Console.Write("Retrieving playlists data");
            var result = new List<YoutubePlaylist>();

            var nextPageToken = "";
            while (nextPageToken != null)
            {
                var listRequest = service.Playlists.List("snippet");
                listRequest.Mine = true;
                listRequest.PageToken = nextPageToken;
                var lists = listRequest.Execute();
                result.AddRange(lists.Items.Select(z => new YoutubePlaylist { Id = z.Id, Title = z.Snippet.Title }));
                nextPageToken = lists.NextPageToken;
                Console.Write(".");
            }
            Console.WriteLine();

            Console.Write("Retrieving playlists entries");
            foreach (var e in result)
            {
                UpdateAllPlaylistItems(e);
                Console.Write(".");
            }
            Console.WriteLine();

            return result;
        }



        [STAThread]
        public static void Main()
        {
            service = Publishing.InitializeYoutube();
            Publishing.SaveList(GetAllPlaylists());
            Publishing.SaveList(GetAllClips());
        }
    }
}
