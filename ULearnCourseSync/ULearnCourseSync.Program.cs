using CoursePublishing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ULearnCourseSync
{
    class SlideData
    {
        public FileInfo Files;
        public string GuidMatch;
        public string IdMatch;
    }


    class Program
    {
        static UlearnSyncSettings Settings;
        static string CourseName;
        static bool Preview;
        static Section Structure;
        
        static Dictionary<Guid, YoutubeClip> Clips;
        static DirectoryInfo StartFolder;
        static List<DirectoryInfo> ExistingDirectories;

        static Regex GUID = new Regex(@"\[Slide\([^\n]+,""([0-9a-fA-F-]+)""\)\]");
        static Regex YoutubeId = new Regex(@"//#video ([^ \t\n\r]+)");


        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Pass the name of the course as the first argument");
                return;
            }

            CourseName = args[0];

            if (args.Length > 1)
            {
                Preview = args[1] == "Preview";
            }

            Settings = Publishing.LoadInitOrEdit<CourseSettings>(-1,CourseName).Ulearn;
            Structure = Publishing.LoadCourseStructure(CourseName);

            Clips =
                (from rel in Publishing.LoadList<VideoToYoutubeClip>()
                 join clip in Publishing.LoadList<YoutubeClip>() on rel.YoutubeId equals clip.Id
                 select new { rel, clip }
                ).ToDictionary(z => z.rel.Guid, z => z.clip);

            StartFolder = new DirectoryInfo(Path.Combine(Settings.Path, "Slides"));

      

            ShowFoldersStructure();
            var slides = GetVideoSlides();
           

        }

        static void ShowFoldersStructure()
        {
            ExistingDirectories = StartFolder.GetDirectories("L*").ToList();
            var dueFolders = Structure
                .Items
                .Sections()
                .Where(z => z.Level == Settings.FoldersLevel)
                .ToList();

            for (int i=ExistingDirectories.Count;i<dueFolders.Count;i++)
            {
                var folderName = string.Format("{0:D3} - {1}",
                    (i + 1) * 10,
                    dueFolders[i].Name);

                var info = StartFolder.CreateSubdirectory(folderName);
                ExistingDirectories.Add(info);
            }

            for (int i = 0; i < ExistingDirectories.Count; i++)
            {
                Console.WriteLine("{0,-30}{1,-30}",
                    ExistingDirectories[i].Name,
                    i < dueFolders.Count ? dueFolders[i].Name : "");
            }
            Console.WriteLine();
        }

        static IEnumerable<SlideData> GetVideoSlides()
        {
            var files = StartFolder.GetFiles("S*", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                var text = File.ReadAllText(f.FullName);

                var guidMatch = GUID.Match(text);
                if (!guidMatch.Success) continue;
                var youtubeMatch = YoutubeId.Match(text);
                if (!youtubeMatch.Success) continue;
                yield return new SlideData
                {
                    Files = f,
                    GuidMatch = guidMatch.Groups[1].Value,
                    IdMatch = youtubeMatch.Groups[1].Value
                };

            }
        }
    }
}
