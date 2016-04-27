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

        public static void SaveCourseStructure(string CourseName,Section root)
        {
            var str= JsonConvert.SerializeObject(root, Formatting.Indented);
            var path = Path.Combine(Env.DataFolder, CourseName + Env.StructureExtension);
            File.WriteAllText(path, str);
            var r = LoadVideoToCourse().ToList();
            var section = r.Where(z => z.CourseGuid == root.Guid).FirstOrDefault();
            if (section == null)
                section = new VideoToCourse { CourseGuid = root.Guid };
            else
                r.Remove(section);
            section.VideoGuids.Clear();
            section.VideoGuids.AddRange(root.Items.OfType<Guid>());
            r.Add(section);
            File.WriteAllText(Env.VideoToCourse, JsonConvert.SerializeObject(r, Formatting.Indented));

        }

        public static Video[] LoadAllVideos()
        {
            return JsonConvert.DeserializeObject<Video[]>(File.ReadAllText(Env.VideoList));
        }

        public static VideoToCourse[] LoadVideoToCourse()
        {
            if (!File.Exists(Env.VideoToCourse)) return new VideoToCourse[0];
            return JsonConvert.DeserializeObject<VideoToCourse[]>(File.ReadAllText(Env.VideoToCourse));
        }

    }
}
