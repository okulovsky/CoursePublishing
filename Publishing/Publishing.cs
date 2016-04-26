using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class Publishing
    {
        public static Section LoadCourseStructure(string CourseName)
        {
            var path = Path.Combine(Env.DataFolder, CourseName + Env.StructureExtension);
            if (!File.Exists(path)) return new Section { Guid = Guid.NewGuid(), Name = CourseName };
            return JsonConvert.DeserializeObject<Section>(File.ReadAllText(path));
        }

        public static Video[] LoadAllVideos()
        {
            return JsonConvert.DeserializeObject<Video[]>(File.ReadAllText(Env.VideoList));
        }

        public static VideoToCourse[] LoadVideoToCourse()
        {
            return JsonConvert.DeserializeObject<VideoToCourse[]>(File.ReadAllText(Env.VideoToCourse));
        }

    }
}
