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
        public StepicSyncSettings Stepic { get; set; }
        public CourseSettings()
        {
        }
    }

    public class YoutubeSyncSettings
    {
        public int[] PlayListLevels { get; set; } = new [] { 0 };
        public string Channel { get; set; }

        public string DescriptionPattern { get; set; } = "";

        public bool IgnorePlaylists { get; set; } = false;

        public YoutubeSyncSettings()
        {
        }
    }

    public class UlearnSyncSettings
    {
        public string Path { get; set; } = "";
        public int FoldersLevel { get; set; } = 2;
    }

    public class StepicSyncSettings
    {
        public int LessonsLevel { get; set; }
        public int ModulesLevel { get; set; }
        public int CourseNumber { get; set; }
        public int TeacherId { get; set; }
    }
}
