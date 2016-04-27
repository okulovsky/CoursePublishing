using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public static class Env
    {
        static Env()
        {
            ProgramLocation=new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.Parent.FullName;
            CredentialsFolder = ProgramLocation+"\\Credentials\\";
            DataFolder = ProgramLocation + "\\Data\\";
            VideoList = DataFolder + "videos.json";
            VideoToCourse = DataFolder + "videoToCourse.json";
        }

        public static readonly string ProgramLocation;
        public static readonly string CredentialsFolder;
        public static readonly string DataFolder;
        public static readonly string VideoList;
        public static readonly string VideoToCourse;
        public static readonly string StructureFileName = "struct";

    }
}
