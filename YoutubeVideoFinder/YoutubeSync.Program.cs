using CoursePublishing;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeSync
{
    class Program
    {

        static YouTubeService service; 
        static void Initialize()
        {
            var credentialsLocation =Publishing.MakePath(Env.CredentialsFolder,"Youtube");
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = Credentials.Current.YoutubeClientId, ClientSecret = Credentials.Current.YoutubeClientSecret},
                new[] { YouTubeService.Scope.Youtube },
                "user",
                CancellationToken.None,
                new FileDataStore(credentialsLocation)).RunSync();

            service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Tuto Editor"
            });
        }


        static List<YoutubeClip> GetAllClips()
        {
            Console.WriteLine("Retrieving video data");

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

        static void Main(string[] args)
        {
            Initialize();
            List<YoutubeClip> clips = null;

            clips = GetAllClips(); Publishing.SaveList(clips);
            clips = Publishing.LoadList<YoutubeClip>();
            var videos = Publishing.LoadList<VideoToCourse>();
            

        }
    }
}
