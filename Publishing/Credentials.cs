﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class Credentials
    {
        public string YoutubeClientId="";
        public string YoutubeClientSecret="";
        public string StepicClientId="";
        public string StepicClientSecret="";

        static Credentials current;

        public static Credentials Current { get { if (current==null) Init(); return current; }}

        static void Init()
        {
            var path = Publishing.MakePath(Env.CredentialsFolder, "secrets.json");
            if (!File.Exists(path))
            {
                var credentials = new Credentials();
                File.WriteAllText(path, JsonConvert.SerializeObject(credentials, Formatting.Indented));
                Process.Start(path).WaitForExit();
            }
            current = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(path));
        }
    }
}
