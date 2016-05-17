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
        public static string Token { get; private set; }
        public const string Api = "https://stepic.org/api";
        public static void Authorize()
        {
            Token = "https://stepic.org/oauth2/token/"
                .WithBasicAuth(Credentials.Current.StepicClientId, Credentials.Current.StepicClientSecret)
                .PostUrlEncodedAsync(new { grant_type = "client_credentials" })
                .ReceiveJson()
                .Now()
                .Select(z => (string)z.access_token)
                .First();
        }

        public static JObject SendVideoNotWorking(FileInfo file)
        {
            return Api
                .AppendPathSegment("videos")
                .WithOAuthBearerToken(Token)
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
            string header = string.Format(headerTemplate, paramName, file.Name, contentType);
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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Api + "/videos");
            request.Headers.Add("Authorization", "Bearer " + Token);
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



        public static StepicEntity Lesson = new StepicEntity("lessons", "lesson","lessons");
        public static StepicEntity Step = new StepicEntity("step-sources", "stepSource", "step-sources");
        public static StepicEntity Section = new StepicEntity("sections", "section", "sections");
        public static StepicEntity Units = new StepicEntity("units", "unit", "units");


       

    }
}
