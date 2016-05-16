using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecodeVideoForStepic
{
    class Program
    {
        const string TutoOutputFolder = "";
        const string RecodedFolder = "";
        const string BatFolder = "";
        const string ffmpeg = "";
        static void Main(string[] args)
        {
            var result = new StringBuilder();

            var readyFiles = new DirectoryInfo(RecodedFolder)
                .GetFiles()
                .Select(z => z.FullName)
                .ToList();


            foreach (var file in
                new DirectoryInfo(TutoOutputFolder)
                .GetFiles("*.avi", SearchOption.TopDirectoryOnly))
            {

                var readyPath = Path.Combine(RecodedFolder, Path.GetFileNameWithoutExtension(file.Name) + "mp4");
                if (readyFiles.Contains(readyPath))
                    continue;
                result.AppendLine($"\"{ffmpeg}\" -i \"{file.FullName}\" -acodec copy -vcodec libx264 -preset slow -crf 25 \"{readyPath}\"");
            }
            File.WriteAllText(Path.Combine(BatFolder, "convertForStepic.bat"), result.ToString());
        }
    }
}
