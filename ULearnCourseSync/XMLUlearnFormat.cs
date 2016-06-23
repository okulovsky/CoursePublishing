using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoursePublishing;

namespace ULearnCourseSync
{
    class XMLUlearnFormat : UlearnFormat
    {
        public string Extension
        {
            get
            {
                return ".lesson.xml";
            }
        }

        public Regex GuidRegex
        {
            get
            {
                return new Regex(@"<id>([0-9a-fA-F-]+)</id>");
            }
        }

        public Regex YoutubeRegex
        {
            get
            {
                return new Regex(@"<youtube>([^ \t\n\r]+)</youtube>");
            }
        }

        public string MakeSlide(string CourseName, YoutubeClip youtubeClip, Video video)
        {
            var template = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Lesson xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""https://ulearn.azurewebsites.net/lesson"">
	<title>{0}</title>
	<id>{1}</id>
    <youtube>{2}</youtube>
	
</Lesson>";

            var text = String.Format(template,
                video.Title,
                video.Guid,
                youtubeClip.Id
                );
            return text;
        }
    }
}
