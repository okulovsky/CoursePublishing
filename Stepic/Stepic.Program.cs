using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl;
using CoursePublishing;
namespace Stepic
{

    class Program
    {

        static string token;
        static string api = "https://stepic.org/api";
        static void Authorize()
        {
            token = "https://stepic.org/oauth2/token/"
                .WithBasicAuth(Credentials.Current.StepicClientId, Credentials.Current.StepicClientSecret)
                .PostUrlEncodedAsync(new { grant_type = "client_credentials" })
                .ReceiveJson()
                .Now()
                .Select(z => (string)z.access_token)
                .First();
        }

        public static dynamic CreateLesson(string title)
        {
            return api
                .AppendPathSegment("lessons")
                .WithOAuthBearerToken(token)
                .PostJsonAsync(new { lesson = new { title = title } })
                .ReceiveJson()
                .Now()
                .Select(z => z.lessons[0])
                .First();
        }

        public static dynamic UpdateLesson(dynamic lesson)
        {
            return api
                .AppendPathSegment("lessons")
                .AppendPathSegment(((long)lesson.id).ToString())
                .WithOAuthBearerToken(token)
                .PutJsonAsync(new { lesson = lesson })
                .ReceiveJson()
                .Now()
                .Select(z => z.lessons[0])
                .First();


        }



        public static void DeleteLesson(long id)
        {
            api
                .AppendPathSegment("lessons")
                .AppendPathSegment(id.ToString())
                .WithOAuthBearerToken(token)
                .DeleteAsync()
                .Now()
                .First();
        }


        public static void PrintJson(object o)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented));
        }


        static void Main()
        {
            Authorize();
            var lesson = CreateLesson("Test");
            PrintJson(lesson);
            return;
            lesson.title = "Test 1";
            lesson = UpdateLesson(lesson);
            PrintJson(lesson);

            DeleteLesson(27043);

        }
    }
}
