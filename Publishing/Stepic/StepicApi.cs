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
using System.Net;
using System.Collections.Specialized;

namespace CoursePublishing
{
    public class StepicApi
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

        public static JObject SendVideoNotWorking(FileInfo file)
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

        static void EmulateForm(HttpWebRequest rq, NameValueCollection nvc, string paramName, FileInfo file, string contentType )
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            rq.ContentType = "multipart/form-data; boundary=" + boundary;
            var rs = rq.GetRequestStream();


            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, "C:\\video.mp4", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            var length = file.Length;
            var sent = 0;

            FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[409600];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
                sent += buffer.Length;
                var percent = sent * 100.0 / length;
                Console.Write("{0} completed                        \r", percent);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();
        }

        public static JObject SendVideo(FileInfo file, string lessonId)
        {
     
            Console.WriteLine("Sending " + file.FullName);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(api + "/videos");
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Method = "POST";
            request.KeepAlive = true;
            request.SendChunked = true;
            var nvc = new NameValueCollection();
            nvc["lesson"] = lessonId;
            EmulateForm(request, nvc, "source", file, "video/mp4");

            var response = request.GetResponse();
            var responseText = "";
            using (var reader = new StreamReader(response.GetResponseStream()))
                responseText = reader.ReadToEnd();
            return JObject.Parse(responseText)["videos"][0] as JObject;
        }



        public static Entity Lesson = new Entity("lessons", "lesson","lessons");
        public static Entity Step = new Entity("step-sources", "stepSource", "step-sources");
        public static Entity Section = new Entity("sections", "section", "sections");
        public static Entity Units = new Entity("units", "unit", "units");


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

            public List<JObject> Get(object p)
            {
                var list = new List<JObject>();
                while(true)
                {
                    int pageNum = 1;
                    var result = api
                        .AppendPathSegment(apiPath)
                        .SetQueryParam("page", pageNum)
                        .SetQueryParams(p)
                        .GetJsonAsync<JObject>()
                        .Now()
                        .Select(z => z)
                        .First();
                    foreach (var e in result[receiveSelector])
                        list.Add(e as JObject);
                    if (!result["meta"]["has_next"].Value<bool>()) break;
                }
                return list;
            }
        }

    }
}
