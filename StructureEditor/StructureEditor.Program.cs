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
        const string CourseName = "LHPS";


        static string PrepareText()
        {
            StringBuilder text = new StringBuilder();
            var root = Publishing.LoadCourseStructure(CourseName);
            var videos = Publishing.LoadAllVideos();
            foreach (var e in root.Items)
            {
                if (e is Section)
                {
                    var section = e as Section;
                    for (int i = 0; i <= section.Level; i++)
                        text.Append("#");
                    text.Append(" ");
                    text.Append(section.Name);
                    text.Append(" | ");
                    text.Append(section.Guid);
                    text.AppendLine();
                    continue;
                }
                if (e is Guid)
                {
                    var guid = (Guid)e;
                    var video = videos.Where(z => z.Guid == guid).FirstOrDefault();
                    if (video == null)
                    {
                        text.AppendLine("\tMissing video " + guid);
                    }
                    else
                    {
                        text.AppendLine("\t" + video.Title + " | " + guid);
                    }
                }
            }
            text.AppendLine();
            text.AppendLine();
            text.AppendLine("---------------------------------------------------------");
            text.AppendLine("Videos below this line are not yet included in any course");
            text.AppendLine();

            var bindedVideos = Publishing.LoadVideoToCourse().SelectMany(z=>z.VideoGuids).Distinct().ToDictionary(z=>z,z=>true);
            var freeVideos = videos.Where(z => !bindedVideos.ContainsKey(z.Guid));
            foreach (var v in freeVideos)
            {
                text.AppendLine("\t" + v.Title + " | " + v.Guid);
            }
            return text.ToString();
        }

        static readonly Regex parser = new Regex("[\\t #]*([^|]*) | (.*)");

        static Tuple<int, string, Guid> ParseRecord(string line)
        {
            var match = parser.Match(line);
            if (!match.Success) throw new Exception();
            var level = match.Groups[1].Value.Count(z => z == '#') - 1;
            return Tuple.Create(level, match.Groups[2].Value, Guid.Parse(match.Groups[3].Value));
        }

        static Section Parse(string text)
        {
            List<Section> roots = new List<Section>();
            bool hasRootSection = false;
            var lines = text.Split('\n');
            for (int i=1;i<=lines.Length;i++)
            {
                var e = lines[i-1];
                if (e.StartsWith("-----")) break;
                if (string.IsNullOrWhiteSpace(e)) continue;
                Tuple<int, string, Guid> record = null;
                try
                {
                    record = ParseRecord(e);
                }
                catch
                {
                    throw new Ex(i, "Can't parse record");
                }

                if (record.Item1 != -1)
                {

                    if (hasRootSection && record.Item1 == 0)
                        throw new Ex(i, "Only one root record is allowed, it is the name of the course");
                    if (roots.Count < record.Item1)
                        throw new Ex(i, "Unexpected section of depth " + record.Item1);

                    if (record.Item1==0)
                    {
                        roots.Add(
                }
            }
        }

        static void Main(string[] args)
        {
            var text = PrepareText();
            File.WriteAllText("temp.txt", text);
            var p = Process.Start("temp.txt");
            p.WaitForExit();

        }
    }
}
