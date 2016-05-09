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
        static void Initialize()
        {
            var credentialsLocation = Publishing.MakePath(Env.CredentialsFolder, "Youtube");
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = Credentials.Current.YoutubeClientId, ClientSecret = Credentials.Current.YoutubeClientSecret },
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


        [STAThread]
        public static void Main()
        {
            Initialize();
            List<YoutubeClip> clips = null;

            //clips = GetAllClips(); Publishing.SaveList(clips);
            clips = Publishing.LoadList<YoutubeClip>();
            var relation = Publishing.LoadList<VideoToYoutubeClip>();
            var videos = Publishing.LoadList<Video>();

            var model = new MatchModel();

            var bound =
                from clip in clips
                join rel in relation on clip.Id equals rel.YoutubeId
                join video in videos on rel.Guid equals video.Guid
                select new { video, clip};

            foreach (var e in bound)
                model.Matches.Add(Tuple.Create(e.video, new YoutubeClipViewModel(e.clip)));

            foreach(var v in videos.Except(bound.Select(z=>z.video)))
            {
                model.Videos.Add(v);
            }

            foreach(var y in clips.Except(bound.Select(z=>z.clip)))
            {
                model.Clips.Add(new YoutubeClipViewModel(y));
            }

            var window = new MainWindow();
            window.DataContext = model;
            (new Application()).Run(window);

            relation = model.Matches.Select(z => new VideoToYoutubeClip(z.Item1.Guid, z.Item2.Clip.Id)).ToList();
            Publishing.SaveList(relation);
        }
    }
}
