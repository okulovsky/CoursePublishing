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


    class UlearnCourseSyncService : PreviewableService
    {
        UlearnSyncSettings Settings;
        string CourseName;
        Section Structure;
        List<Section> TopicsForFolders;
        Dictionary<Guid, Video> Videos;
        Dictionary<Guid, YoutubeClip> Clips;
        Dictionary<string, Guid> ClipToGuid;
        DirectoryInfo StartFolder;
        Dictionary<Guid, string> SectionToFolder;


        static Regex GuidRegex = new Regex(@"\[Slide\(.+, ?""([0-9a-fA-F-]+)""\)\]");
        static Regex YoutubeRegex = new Regex(@"//#video ([^ \t\n\r]+)");


        static void Main(string[] args)
        {
            new UlearnCourseSyncService().Run(args);
        }

        public void Run(string[] args)
        {
            CourseName = Publishing.GetCourseNameFromArgs(args);

            Initialize();
        }

        void Initialize()
        { 
            Settings = Publishing.Courses[CourseName].LoadInitOrEdit<CourseSettings>(3).Ulearn;
            Structure = Publishing.Courses[CourseName].Load<Structure>();
            TopicsForFolders = Structure.Items.Sections().Where(z => z.Level == Settings.FoldersLevel).ToList();

            Clips =
                (from rel in Publishing.Common.LoadList<VideoToYoutubeClip>()
                 join clip in Publishing.Common.LoadList<YoutubeClip>() on rel.YoutubeId equals clip.Id
                 select new { rel, clip }
                ).ToDictionary(z => z.rel.Guid, z => z.clip);

            ClipToGuid = Publishing
                .Common
                .LoadList<VideoToYoutubeClip>()
                .ToDictionary(z => z.YoutubeId, z => z.Guid);

            Videos =
                (from guid in Structure.Items.VideoGuids()
                 join video in Publishing.Common.LoadList<Video>() on guid equals video.Guid
                 select video
                 ).ToDictionary(z => z.Guid, z => z);
          
            StartFolder = new DirectoryInfo(Path.Combine(Settings.Path, "Slides"));

      

            ShowFoldersStructure();
            ProcessSlides();

            AskAndExecute();
        }

     
        string FolderNameFor(Section section)
        {
            var index = TopicsForFolders.IndexOf(section);
            if (index == -1)
                throw new Exception("Folders must be created only for sections at the correct level");
            return string.Format("L{0:D3} - {1}",
                    (index+1)*10,
                    section.Name.Trim());
        }

        void ShowFoldersStructure()
        {
            SectionToFolder = new Dictionary<Guid, string>();
            var ExistingDirectories = StartFolder.GetDirectories("L*").ToList();
            var dueFolders = Structure
                .Items
                .Sections()
                .Where(z => z.Level == Settings.FoldersLevel)
                .ToList();

            for (int i = 0; i < ExistingDirectories.Count; i++)
            {
                Console.WriteLine("{0,-30}{1,-30}",
                    i < ExistingDirectories.Count ? ExistingDirectories[i].Name : "",
                    i < dueFolders.Count ? dueFolders[i].Name : "");

                if (i < ExistingDirectories.Count && i < dueFolders.Count)
                    SectionToFolder[dueFolders[i].Guid] = ExistingDirectories[i].FullName;
            }

            for (int i=ExistingDirectories.Count;i<dueFolders.Count;i++)
            {
                var folderName = FolderNameFor(dueFolders[i]);
                SectionToFolder[dueFolders[i].Guid] = folderName;
                AddAction("Creating folder folderName", () => CreatingSubdirectory(folderName, dueFolders[i].Name));
            }


            Console.WriteLine();
        }

        void CreatingSubdirectory(string folderName, string name)
        {
            var info = StartFolder.CreateSubdirectory(folderName);

            File.WriteAllText(
                Path.Combine(info.FullName, "Title.txt"),
                name);
        }

            IEnumerable<SlideData> GetVideoSlides()
        {
            var files = StartFolder.GetFiles("S*", SearchOption.AllDirectories);
            foreach (var f in files)
            {
           
                var text = File.ReadAllText(f.FullName);

                var guidMatch = GuidRegex.Match(text);

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

         void ProcessSlides()
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
                        AddAction(
                            $"Changing Youtube reference in {e.RelativePath}",
                            ()=>ChangeYoutubeClip(e, Clips[e.GuidMatch].Id));
                        continue;
                    }
                    continue;
                }


                if (ClipToGuid.ContainsKey(e.IdMatch))
                {
                    var realGuid = ClipToGuid[e.IdMatch];
                    AddAction(
                        $"Changing GUID in {e.RelativePath}",
                        ()=>ChangeGuid(e, realGuid));
                    VideoGuids[realGuid].Add(e.RelativePath);
                }
            }

            GenerateSlides(VideoGuids.Where(z => z.Value.Count == 0).Select(z => z.Key));
        }

        private void GenerateSlides(IEnumerable<Guid> guids)
        {
            var data =
                 Structure
                .ItemsWithPathes
                .Where(z => z.Item.VideoGuid.HasValue)
                .ToDictionary(z => z.Item.VideoGuid.Value, z => z);
            foreach(var e in guids)
            {
                if (!data.ContainsKey(e)) throw new Exception("This should be impossible");
                var d = data[e];
                var section = d.Path.Where(z => z.Section.Level == Settings.FoldersLevel).First().Section;
                var folderName = SectionToFolder[section.Guid];
                var slideNum = section.Items.VideoGuids().ToList().IndexOf(d.Item.VideoGuid.Value);
                var slideName = string.Format("S{0:D3} - {1}.cs",
                    (slideNum + 1) * 10,
                    Videos[e].Title.Trim());
                var relativePath = Path.Combine(folderName, slideName);
                AddAction($"Creating slide {relativePath}", () => CreateSlide(relativePath, e, Clips[e], Videos[e]));
            }
        }

        private void CreateSlide(string absolutePath, Guid e, YoutubeClip youtubeClip, Video video)
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
                e,
                csName,
                youtubeClip.Id);
            File.WriteAllText(absolutePath, text);
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
            var text = File.ReadAllText(slide.Files.FullName);
            text = ChangeWithRegexp(text, GuidRegex, 1, guid.ToString());
            File.WriteAllText(slide.Files.FullName, text);
        }

        private static void ChangeYoutubeClip(SlideData slide, string id)
        {
           var text = File.ReadAllText(slide.Files.FullName);
            text = ChangeWithRegexp(text, YoutubeRegex, 1, id);
            File.WriteAllText(slide.Files.FullName, text);
        }
    }
}
