using Flurl.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Flurl;
using System.Threading.Tasks;

namespace Stepic
{
    static class MyExtension
    {
        public static IEnumerable<T> Now<T>(this Task<T> task)
        {
            task.Wait();
            yield return task.Result;
        }

        public static void PrintJson<T>(this IEnumerable<T> o)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(o.First(), Newtonsoft.Json.Formatting.Indented));
        }


        public static Task<HttpResponseMessage> PostFileAsync(this FlurlClient client, string filepath)
        {
            var data = File.ReadAllBytes(filepath);
            var content = new ByteArrayContent(data);
            content.Headers.Add("Content-Type", "application/octet-stream");
            content.Headers.Add("Content-Length", data.Length.ToString());
            return client.SendAsync(HttpMethod.Post, content: content);
        }

    }

}
