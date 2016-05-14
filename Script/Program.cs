using CoursePublishing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Script
{
    class Program
    {
        static void RecreateVideoToCourse()
        {
            foreach(var e in new[] { "OOP", "LHPS", "Hackerdom", "AIML","BP1" })
            {
                var s = Publishing.Courses[e    ].Load<Structure>();
                Publishing.SaveCourseStructure(s, e);
            }
        }

        static void StandardizeGuid()
        {
            var path = @"C:\Users\Yura\Desktop\OldPublishings\BasicProgramming\Part01\BasicProgramming\Slides";
            var dinfo = new DirectoryInfo(path);
            Regex GuidRegex = new Regex(@"\[Slide\((.+), ?""\{([0-9a-fA-F-]+)\}""\)\]");

            foreach (var finfo in  dinfo.GetFiles("S*",SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(finfo.FullName);
                if (!GuidRegex.Match(text).Success) continue;
                if (!text.Contains("//#video")) continue;
                text = GuidRegex.Replace(text, @"[Slide($1,""$2"")]");
                File.WriteAllText(finfo.FullName, text);
            }

        }

        static void Main() { }
    }
}
