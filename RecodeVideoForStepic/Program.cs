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
        const string TutoOutputFolder = @"H:\LHPS\Output";
        const string RecodedFolder = @"H:\LHPS\StepicOutput";
        const string BatFolder = @"..\..\..\..\bats";
        const string ffmpeg = @"C:\ffmpeg\bin\ffmpeg.exe";
        static void Main(string[] args)
        {
            var result = new StringBuilder();
            result.AppendLine("chcp 65001");

            var readyFiles = new DirectoryInfo(RecodedFolder)
                .GetFiles()
                .Select(z => z.FullName)
                .ToList();


            foreach (var file in
                new DirectoryInfo(TutoOutputFolder)
                .GetFiles("*.avi", SearchOption.TopDirectoryOnly))
            {

                var readyPath = Path.Combine(RecodedFolder, Path.GetFileNameWithoutExtension(file.Name) + ".mp4");
                if (readyFiles.Contains(readyPath))
                    continue;
                result.AppendLine($"\"{ffmpeg}\" -i \"{file.FullName}\" -acodec copy -vcodec libx264 -preset slow -crf 25 -y \"{readyPath}\"");
            }
            var batPath = Path.Combine(BatFolder, "convertForStepic.bat");
            using (var writer = new StreamWriter(File.Open(batPath, FileMode.OpenOrCreate,FileAccess.Write), new UTF8Encoding(false)))
            {
                writer.WriteLine(result.ToString());
            }
        }
    
    }
}
