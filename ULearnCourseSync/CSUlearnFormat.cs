using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoursePublishing;

namespace ULearnCourseSync
{

    class CsUlearnFormat : UlearnFormat
    {
        public string Extension
        {
            get
            {
                return ".cs";
            }
        }

        public Regex GuidRegex
        {
            get
            {
                return guidRegex;
            }
        }

        public Regex YoutubeRegex
        {
            get
            {
                return youtubeRegex;
            }
        }

        public string MakeSlide(string CourseName, YoutubeClip youtubeClip, Video video)
        {
            var template = @"
using System;
using System.IO;
using System.Linq;
using uLearn;   

namespace {0}
{{
    [Slide(@""{1}"", ""{2}"")]
    public class {3}
    {{
        //#video {4}
    }}
}}";
            var csName = video.Title.Replace(" ", "_").Replace(".", "_").Replace(",", "_");
            var text = string.Format(template,
                CourseName,
                video.Title,
                video.Guid,
                csName,
                youtubeClip.Id);
            return text;
        }


        Regex guidRegex = new Regex(@"\[Slide\(.+, ?""([0-9a-fA-F-]+)""\)\]");
        Regex youtubeRegex = new Regex(@"//#video ([^ \t\n\r]+)");

    }

}
