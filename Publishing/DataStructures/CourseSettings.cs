using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class CourseSettings
    {
        public YoutubeSyncSettings Youtube { get; set; } = new YoutubeSyncSettings();
        public UlearnSyncSettings Ulearn { get; set; } = new UlearnSyncSettings();
        public CourseSettings()
        {
        }
    }

    public class YoutubeSyncSettings
    {
        public int[] PlayListLevels { get; set; } = new [] { 0 };

        public YoutubeSyncSettings()
        {
        }
    }

    public class UlearnSyncSettings
    {
        public string Path { get; set; } = "";
        public int FoldersLevel { get; set; } = 2;
    }


}
