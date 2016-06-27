using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            string CourseName = "Testing";
            //      StructureEditor.Program.Main(new[] { CourseName });
            //      YoutubeSync.YoutubeSync.Main();
            //      YoutubeSync.Match.Main(new[] { CourseName });
            //YoutubeCourseSync.YoutubeCourseSyncService.Main(new[] { CourseName });
            ULearnCourseSync.UlearnCourseSyncService.Main(new[] { CourseName });
        }
    }
}
