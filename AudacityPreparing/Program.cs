using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudacityPreparing
{
    class Program
    {
        const string InputNow = @"F:\LHPS\Input";
        const string ffmpeg = @"C:\ffmpeg\bin\ffmpeg.exe";
        const string sikuli = @"C:\Sikuli\";
        const string whereToStoreBat = @".\";

        static void Main(string[] args)
        {
            var directory = new DirectoryInfo(InputNow);
            var files = directory.GetFiles("*", SearchOption.AllDirectories);
            var goodNames = new[] { "face.mp4", "desktop.avi", "hash", "local.tuto" };
            var badNames = new[] {"cleaned.mp3","voice.mp3","desktop.ini" };
            var extract = "";
            var clean = "";
            var toMp3 = "";

            foreach (var e in files)
            {
                if (!goodNames.Contains(e.Name) && !badNames.Contains(e.Name))
                    Console.WriteLine(e.FullName);
                if (badNames.Contains(e.Name))
                {
                    Console.WriteLine($"Deleting {e.FullName}");
                    File.Delete(e.FullName);
                }
                if (e.Name=="face.mp4")
                {
                    var path = e.Directory.FullName;
                    extract += $"{ffmpeg} -i \"{e.FullName}\" -y \"{path}\\input.wav\"\r\n";
                    clean += $"{sikuli} \"{path}\\input.wav\" \"{path}\\clean.wav\"\r\n";
                    toMp3 += $"{ffmpeg} -i \"{0}\\clean.wav\" -ar 44100 -ac 2 -ab 192k -f mp3 -qscale 0 \"{path}\\cleaned.mp3\" -y\r\n";
                }
            }

            File.WriteAllText($"{whereToStoreBat}\\extract.bat", extract);
            File.WriteAllText($"{whereToStoreBat}\\clean.bat", clean);
            File.WriteAllText($"{whereToStoreBat}\\tomp3.bat", toMp3);
        }
    }
}
