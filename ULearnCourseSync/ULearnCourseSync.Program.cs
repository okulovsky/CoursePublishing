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
        public string RelativePath;
        public FileInfo Files;
        public Guid GuidMatch;
        public string IdMatch;
        public override string ToString()
        {
            return RelativePath;
        }
    }


    class Program
    {
        static UlearnSyncSettings Settings;
        static string CourseName;
        static bool Preview;
        static Section Structure;
        static List<Section> TopicsForFolders;
        static Dictionary<Guid, YoutubeClip> Clips;
        static Dictionary<string, Guid> ClipToGuid;
        static DirectoryInfo StartFolder;
        static List<DirectoryInfo> ExistingDirectories;


        static Regex GuidRegex = new Regex(@"\[Slide\([^\n]+,""([\""]+)""\)\]");
        static Regex YoutubeRegex = new Regex(@"//#video ([^ \t\n\r]+)");


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
            TopicsForFolders = Structure.Items.Sections().Where(z => z.Level == Settings.FoldersLevel).ToList();

            Clips =
                (from rel in Publishing.LoadList<VideoToYoutubeClip>()
                 join clip in Publishing.LoadList<YoutubeClip>() on rel.YoutubeId equals clip.Id
                 select new { rel, clip }
                ).ToDictionary(z => z.rel.Guid, z => z.clip);

            ClipToGuid = Publishing
                .LoadList<VideoToYoutubeClip>()
                .ToDictionary(z => z.YoutubeId, z => z.Guid);

          
            StartFolder = new DirectoryInfo(Path.Combine(Settings.Path, "Slides"));

      

            ShowFoldersStructure();
            ProcessSlides();
        }

     
        static string FolderNameFor(Section section)
        {
            var index = TopicsForFolders.IndexOf(section);
            if (index == -1)
                throw new Exception("Folders must be created only for sections at the correct level");
            return string.Format("L{0:D3} - {1}",
                    (index+1)*10,
                    section.Name.Trim());
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
                var folderName = FolderNameFor(dueFolders[i]);

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

                var guidMatch = GuidRegex.Match(text);

                if (f.Name.StartsWith("S060"))
                {
                    Console.Write(guidMatch.Success);
                    Console.Write("");
                }


                if (!guidMatch.Success) continue;
                var youtubeMatch = YoutubeRegex.Match(text);
                if (!youtubeMatch.Success) continue;
                yield return new SlideData
                {
                    Files = f,
                    GuidMatch = Guid.Parse(guidMatch.Groups[1].Value),
                    IdMatch = youtubeMatch.Groups[1].Value,
                    RelativePath = f.FullName.Substring(StartFolder.FullName.Length)
                };

            }
        }

        static void ProcessSlides()
        {
            var VideoGuids = Structure.Items.VideoGuids().ToDictionary(z => z, z => new List<string>());
            var slides = GetVideoSlides().ToList();
            foreach (var e in slides)
            {
                if (VideoGuids.ContainsKey(e.GuidMatch))
                {
                    if (VideoGuids[e.GuidMatch].Count != 0)
                    {
                        Console.WriteLine($"ERROR: Duplicating GUID {e.RelativePath}, conflict with {VideoGuids[e.GuidMatch][0]}");
                        continue;
                    }
                    VideoGuids[e.GuidMatch].Add(e.RelativePath);
                    if (!Clips.ContainsKey(e.GuidMatch))
                    {
                        Console.WriteLine($"ERROR: no video is referenced for {e.RelativePath} in index, however file contains link to {e.IdMatch}");
                        continue;
                    }
                    if (Clips[e.GuidMatch].Id != e.IdMatch)
                    {
                        ChangeYoutubeClip(e, Clips[e.GuidMatch].Id);
                        continue;
                    }
                    continue;
                }


                if (ClipToGuid.ContainsKey(e.IdMatch))
                {
                    ChangeGuid(e, ClipToGuid[e.IdMatch]);
                }
            }

            GenerateSlides(VideoGuids.Where(z => z.Value.Count == 0).Select(z => z.Key));
        }

        private static void GenerateSlides(IEnumerable<Guid> guids)
        {
            var data =
                 Structure
                .ItemsWithPathes
                .Where(z => z.Item.VideoGuid.HasValue)
                .ToDictionary(z => z.Item.VideoGuid.Value, z => z);
            var Videos = Publishing.LoadList<Video>().ToDictionary(z => z.Guid, z => z);
            foreach(var e in guids)
            {
                if (!data.ContainsKey(e)) throw new Exception("This should be impossible");
                var d = data[e];
                var section = d.Path.Where(z => z.Section.Level == Settings.FoldersLevel).First().Section;
                var folderName = FolderNameFor(section);
                var slideNum = section.Items.VideoGuids().ToList().IndexOf(d.Item.VideoGuid.Value);
                var slideName = string.Format("S{0:D3} - {1}.cs",
                    (slideNum + 1) * 10,
                    Videos[e].Title.Trim());
                var relativePath = Path.Combine(folderName, slideName);
                Console.Write($"Creating slide {relativePath}... ");
                if (Preview)
                {
                    Console.WriteLine("Previewed");
                    continue;
                }
                CreateSlide(relativePath, e, Clips[e]);
                Console.WriteLine("Done");
            }
        }

        private static void CreateSlide(string relativePath, Guid e, YoutubeClip youtubeClip)
        {
            var fullPath = Path.Combine(Settings.Path, relativePath);
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
            var csName = youtubeClip.Name.Replace(" ", "_").Replace(".", "_").Replace(",", "_");
            var text = string.Format(template,
                CourseName,
                youtubeClip.Name,
                e,
                csName,
                youtubeClip.Id);
            File.WriteAllText(fullPath, text);
        }
                

        static string ChangeWithRegexp(string text, Regex regex, int groupNumber, string newValue)
        {
            var match = regex.Match(text);
            var newText =
                text.Substring(0, match.Groups[groupNumber].Index)
                + newValue
                + text.Substring(match.Groups[groupNumber].Index + match.Groups[1].Length);
            return newText;
        }

        private static void ChangeGuid(SlideData slide, Guid guid)
        {
            Console.Write($"Changing GUID in {slide.RelativePath}... ");
            if (Preview)
            {
                Console.WriteLine("Previewed");
                return;
            }

            var text = File.ReadAllText(slide.Files.FullName);
            text = ChangeWithRegexp(text, GuidRegex, 1, guid.ToString());
            File.WriteAllText(slide.Files.FullName, text);
            Console.WriteLine("Done");

        }

        private static void ChangeYoutubeClip(SlideData slide, string id)
        {
            Console.Write($"Changing Youtube reference in {slide.RelativePath}... ");
            if (Preview)
            {
                Console.WriteLine("Previewed");
                return;
            }

            var text = File.ReadAllText(slide.Files.FullName);
            text = ChangeWithRegexp(text, YoutubeRegex, 1, id);
            File.WriteAllText(slide.Files.FullName, text);
            Console.WriteLine("Done");

        }
    }
}
