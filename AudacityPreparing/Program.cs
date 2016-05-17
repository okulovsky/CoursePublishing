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
        const string InputNow = @"H:\LHPS\Input";
        const string ffmpeg = @"C:\ffmpeg\bin\ffmpeg.exe";
        const string sikuli = @"""C:\Users\user\Desktop 3\Sikuli\runsikulix.cmd"" -r ""C:\Users\user\Desktop 3\reduce_noise\reduce_noise.skl"" -- ""C:\Program Files (x86)\Audacity\audacity.exe""";
        const string whereToStoreBat = @"..\..\..\..\bats";

        static void Main(string[] args)
        {
            var directory = new DirectoryInfo(InputNow);
            var files = directory.GetFiles("*", SearchOption.AllDirectories);
            var goodNames = new[] { "face.mp4", "desktop.avi", "hash", "local.tuto" };
            var badNames = new[] {"cleaned.mp3","voice.mp3","desktop.ini","clean.wav" };
            var extract = "";
            var delete = "";
            var clean = "";
            var toMp3 = "";

            foreach (var e in files)
            {
                if (!goodNames.Contains(e.Name) && !badNames.Contains(e.Name))
                    Console.WriteLine(e.FullName);
                if (badNames.Contains(e.Name))
                {
                    delete += $"del \"{e.FullName}\"\r\n";

                }
                if (e.Name=="face.mp4")
                {
                    var path = e.Directory.FullName;
                    extract += $"{ffmpeg} -i \"{e.FullName}\" -y \"{path}\\input.wav\"\r\n";
                    clean += $"call {sikuli} \"{path}\\input.wav\" \"{path}\\clean.wav\"\r\ntimeout 5\r\n";
                    toMp3 += $"{ffmpeg} -i \"{path}\\clean.wav\" -ar 44100 -ac 2 -ab 192k -f mp3 -qscale 0 \"{path}\\cleaned.mp3\" -y\r\n";
                }
            }

            File.WriteAllText($"{whereToStoreBat}\\extract.bat", extract);
            File.WriteAllText($"{whereToStoreBat}\\clean.bat", clean);
            File.WriteAllText($"{whereToStoreBat}\\tomp3.bat", toMp3);
            File.WriteAllText($"{whereToStoreBat}\\delRedundand.bat", delete);
        }
    }
}
