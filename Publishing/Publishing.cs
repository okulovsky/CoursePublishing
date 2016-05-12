using CoursePublishing.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class Publishing
    {

        public static string MakePath(params string[] a)
        {
            var f = Path.Combine(a);
            var directory = new FileInfo(f).Directory.FullName;
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return f;
        }

        public static IOManager Common=new IOManager();
        public static Repository Courses = new Repository("Courses");
        public static Repository Channel = new Repository("Channels");

        public static YouTubeService InitializeYoutube(string channelName)
        {
            Console.WriteLine($"Please authorize the program for channel {channelName}");
            var credentialsLocation = Publishing.MakePath(Env.CredentialsFolder, "Youtube",channelName);
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = Credentials.Current.YoutubeClientId, ClientSecret = Credentials.Current.YoutubeClientSecret },
                new[] { YouTubeService.Scope.Youtube },
                "user",
                CancellationToken.None,
                new FileDataStore(credentialsLocation)).RunSync();

            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Tuto Editor"
            });
            return service;
        }

    }
}
