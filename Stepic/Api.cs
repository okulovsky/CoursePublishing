using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl;
using CoursePublishing;
using System.IO;

namespace Stepic
{
    public class Api
    {
        static string token;
        static string api = "https://stepic.org/api";
        public static void Authorize()
        {
            token = "https://stepic.org/oauth2/token/"
                .WithBasicAuth(Credentials.Current.StepicClientId, Credentials.Current.StepicClientSecret)
                .PostUrlEncodedAsync(new { grant_type = "client_credentials" })
                .ReceiveJson()
                .Now()
                .Select(z => (string)z.access_token)
                .First();
        }

        public static JObject SendVideo(FileInfo file)
        {
            return api
                .AppendPathSegment("videos")
                .WithOAuthBearerToken(token)
                .PostFileAsync(file.FullName)
                .ReceiveJson<JObject>()
                .Now()
                .Select(z => z["videos"][0] as JObject)
                .First();
        }



        public static Entity Lesson = new Entity("lessons", "lesson","lessons");
        public static Entity Step = new Entity("step-sources", "stepSource", "step-sources");

        public class Entity
        {
            public Entity(string apiPath, string sendSelector, string receiveSelector)
            {
                this.apiPath=apiPath;
                this.sendSelector = sendSelector;
                this.receiveSelector = receiveSelector;
            }
            readonly string apiPath;
            readonly string sendSelector;
            readonly string receiveSelector;

            JObject CreateObject(object inner)
            {
                var obj = new JObject();
                obj[sendSelector] = JObject.FromObject(inner);
                return obj;
            }

            public JObject Create(object data)
            {
                var str = CreateObject(data).ToString();

                var url = api
                    .AppendPathSegment(apiPath);
                
                return url            
                    .WithOAuthBearerToken(token)
                    .PostJsonAsync(JObject.Parse(str))
                    .ReceiveJson<JObject>()
                    .Now()
                    .Select(z=>z[receiveSelector][0] as JObject)
                    .First();
            }

            public JObject Update(object data)
            {
                var obj = CreateObject(data);

                return api
                   .AppendPathSegment(apiPath)
                   .AppendPathSegment(obj[sendSelector]["id"].Value<string>())
                   .WithOAuthBearerToken(token)
                   .PutJsonAsync(obj)
                   .ReceiveJson()
                   .Now()
                   .Select(z => z[receiveSelector][0] as JObject)
                   .First();
            }

            public void Delete(string id)
            {
                    api
                    .AppendPathSegment(apiPath)
                    .AppendPathSegment(id.ToString())
                    .WithOAuthBearerToken(token)
                    .DeleteAsync()
                    .Now()
                    .First();
            }

            public void Delete(JToken idToken)
            {
                Delete(idToken.Value<string>());
            }

        }

    }
}
