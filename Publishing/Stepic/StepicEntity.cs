﻿using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class StepicEntity
    {
        public StepicEntity(string apiPath, string sendSelector, string receiveSelector)
        {
            this.apiPath = apiPath;
            this.sendSelector = sendSelector;
            this.receiveSelector = receiveSelector;
            api = StepicApi.Api;
        }
        readonly string api;
        readonly string apiPath;
        readonly string sendSelector;
        readonly string receiveSelector;

        public override string ToString()
        {
            return apiPath;
        }

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
                .WithOAuthBearerToken(StepicApi.Token)
                .PostJsonAsync(JObject.Parse(str))
                .ReceiveJson<JObject>()
                .Now()
                .Select(z => z[receiveSelector][0] as JObject)
                .First();
        }

        public JObject Update(object data)
        {
            var obj = CreateObject(data);
            Console.WriteLine(obj);
            return api
               .AppendPathSegment(apiPath)
               .AppendPathSegment(obj[sendSelector]["id"].Value<string>())
               .WithOAuthBearerToken(StepicApi.Token)
               .PutJsonAsync(obj)
               .ReceiveJObject()
               .Now()
               .Select(z => z[receiveSelector][0] as JObject)
               .First();
        }

        public void Delete(string id)
        {
            api
            .AppendPathSegment(apiPath)
            .AppendPathSegment(id.ToString())
            .WithOAuthBearerToken(StepicApi.Token)
            .DeleteAsync()
            .Now()
            .First();
        }

        public void Delete(JToken idToken)
        {
            Delete(idToken.Value<string>());
        }

        public List<JObject> GetByUrl(Func<int,string> urlMaker)
        {
            var list = new List<JObject>();
            int pageNum = 1;
            while (true)
            {

                var api1 = urlMaker(pageNum);

                var result = api1
                    .WithOAuthBearerToken(StepicApi.Token)
                    .GetJsonAsync<JObject>()
                    .Now()
                    .Select(z => z)
                    .First();
                foreach (var e in result[receiveSelector])
                    list.Add(e as JObject);
                var next = result["meta"]["has_next"].Value<bool>();
                if (!next) break;
                pageNum++;
            }
            return list;
        }


        public List<JObject> GetByRequestString(object p)
        {
            return GetByUrl(pageNum => api
                    .AppendPathSegment(apiPath)
                    .SetQueryParam("page", pageNum)
                    .SetQueryParams(p)
                    );
            
        }

        public JObject GetById(int id)
        {
            return GetByUrl(pageNum => api
                    .AppendPathSegment(apiPath)
                    .AppendPathSegment(id.ToString())
                    .SetQueryParam("page", pageNum))[0];
        }
    }
}
