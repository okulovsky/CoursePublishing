using CoursePublishing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StructureEditor
{
    class Ex : Exception
    {
        public int Line;
        public Ex(int line, string message) : base(message) { Line = line; }
    }

    class Program
    {
        static string CourseName;


        static string VideoString(Video video)
        {
            return string.Format("\t{0}-{1} : {2} | {3}", video.OriginalLocation, video.EpisodeNumber, video.Title, video.Guid);
        }

        static string PrepareText()
        {
            StringBuilder text = new StringBuilder();
            var root = Publishing.Courses[CourseName].Load<Structure>();
            var videos = Publishing.Common.LoadList<Video>();
            foreach (var e in root.Items)
            {
                if (e.Section!=null)
                {
                    var section = e.Section;
                    for (int i = 0; i <= section.Level; i++)
                        text.Append("#");
                    text.Append(" ");
                    text.Append(section.Name);
                    text.Append(" | ");
                    text.Append(section.Guid);
                    text.AppendLine();
                    continue;
                }
                if (e.VideoGuid!=null)
                {
                    var guid = e.VideoGuid;
                    var video = videos.Where(z => z.Guid == guid).FirstOrDefault();
                    if (video == null)
                    {
                        text.AppendLine("\tMissing video " + guid);
                    }
                    else
                    {
                       text.AppendLine(VideoString(video));
                    }
                }
            }
            text.AppendLine();
            text.AppendLine();
            text.AppendLine("---------------------------------------------------------");
            text.AppendLine("Videos below this line are not yet included in any course");
            text.AppendLine();

            var bindedVideos = Publishing
                .Common
                .LoadList<VideoToCourse>()
                .SelectMany(z=>z.VideoGuids)
                .Distinct()
                .ToDictionary(z=>z,z=>true);
            var freeVideos = videos.Where(z => !bindedVideos.ContainsKey(z.Guid));
            foreach (var v in freeVideos.OrderBy(z=>z.OriginalLocation).ThenBy(z=>z.EpisodeNumber))
            {
                text.AppendLine(VideoString(v));
            }
            return text.ToString();
        }

        static readonly Regex parser = new Regex("^([ #\\t]*)([^\\|]+)\\|?(.*)$");

        static Tuple<int, string, Guid?> ParseRecord(string line)
        {
            var match = parser.Match(line);
            if (!match.Success) throw new Exception();
            var level = match.Groups[1].Value.Count(z => z == '#') - 1;

            Guid parsedGuid;
            Guid? resultingGuid=null;
            if (Guid.TryParse(match.Groups[3].Value, out parsedGuid))
            {
                resultingGuid=parsedGuid;
            }
            

            return Tuple.Create(level, match.Groups[2].Value, resultingGuid);
        }

        static void TestRegexp()
        {
            var guid = Guid.Parse("19D68CA8-2C65-495D-8F4B-EFF6546C5275");
            Console.WriteLine(ParseRecord("abc | " + guid.ToString()));
            Console.WriteLine(ParseRecord("## abc | " + guid.ToString()));
            Console.WriteLine(ParseRecord("\t\t abc | " + guid.ToString()));
            Console.WriteLine(ParseRecord("# abc | "));
            Console.WriteLine(ParseRecord("# abc"));
            return;
        }

        static Section Parse(string text)
        {
            var videos = Publishing.Common.LoadList<Video>();
           
            List<Section> roots = new List<Section>();
            bool hasRootSection = false;
            var lines = text.Split('\n');
            for (int i = 1; i <= lines.Length; i++)
            {
                var e = lines[i - 1];
                if (e.StartsWith("-----")) break;
                if (string.IsNullOrWhiteSpace(e)) continue;
                Tuple<int, string, Guid?> record = null;
                try
                {
                    record = ParseRecord(e);
                }
                catch
                {
                    throw new Ex(i, "Can't parse record");
                }

                if (record.Item1 == 0)
                {
                    if (hasRootSection)
                        throw new Ex(i, "Only one root record is allowed, it is the name of the course");
                    if (!record.Item3.HasValue)
                        throw new Ex(i, "Don't correct GUID of the course");
                    var topic = new Structure { Name = record.Item2.Trim(), Level = record.Item1, Guid=record.Item3.Value };
                    roots.Add(topic);
                    continue;
                   
                }

                if (record.Item1 != -1)
                {

                    if (roots.Count < record.Item1)
                        throw new Ex(i, "Unexpected section of depth " + record.Item1);
                    while (roots.Count > record.Item1)
                        roots.RemoveAt(roots.Count - 1);
                    var topic = new Section { Name = record.Item2.Trim(), Level = record.Item1 };
                    if (record.Item3.HasValue) topic.Guid = record.Item3.Value;
                    else topic.Guid = Guid.NewGuid();

                    if (roots[roots.Count-1].Videos.Count != 0)
                        throw new Ex(i, "Each section may contain either video or other sections");

                    topic.Order = roots[roots.Count - 1].Sections.Count;
                    roots[roots.Count - 1].Sections.Add(topic);

                    roots.Add(topic);
                    continue;
                }
                if(!record.Item3.HasValue)
                    throw new Ex(i,"Missed guid in video record");
                var v = videos.Where(z => z.Guid == record.Item3.Value).FirstOrDefault();
                if (v==null)
                    throw new Ex(i, "Wrong guid in video record");
                if (roots.Count==0)
                    throw new Ex(i,"Unexpected video record, root section is expected");
                if (roots[roots.Count - 1].Sections.Count != 0)
                    throw new Ex(i, "Each section may contain either video or other section");
                roots[roots.Count - 1].Videos.Add(v.Guid);
            }
            return roots[0];
        }

        static void SaveCourseStructure(Structure root)
        {
            Publishing.Courses[CourseName].Save(root);

            var r = Publishing.Common.LoadList<VideoToCourse>();
            var section = r.Where(z => z.CourseGuid == root.Guid).FirstOrDefault();
            if (section == null)
                section = new VideoToCourse { CourseGuid = root.Guid };
            else
                r.Remove(section);
            section.VideoGuids.Clear();
            section.VideoGuids.AddRange(root.Items.VideoGuids());
            r.Add(section);

            Publishing.Common.SaveList(r);
        }

        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Pass the name of the course as the first argument");
                return;
            }
            CourseName = args[0];

            File.WriteAllText("temp.txt", PrepareText());
            
            while (true)
            {
                var p = Process.Start("temp.txt");
                p.WaitForExit();
                var text = File.ReadAllText("temp.txt");
                Section root = null;
                try
                {
                    root = Parse(text);
                }
                catch (Ex e)
                {
                    Console.WriteLine("Error while parsing document in line " + e.Line + ": " + e.Message);
                    Console.WriteLine("Press ESC to exit editor, or any other key to reopen notepad");
                    Console.WriteLine();
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) return;
                    continue;
                }
                SaveCourseStructure(root as Structure);
                return;

                //text = PrepareText();File.WriteAllText("temp.txt", text); //for debugging
            }
        }
    }
}
