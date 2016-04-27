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

        public static string MakePath(params string[] a)
        {
            var f = Path.Combine(a);
            var directory = new FileInfo(f).Directory.FullName;
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return f;
        }

        #region Simple operations

        static string GetPath<T>(string course=null, string customName=null)
        {
            if (customName == null)
                customName = typeof(T).Name;
            var path = Env.DataFolder;
            if (course != null)
                path = Path.Combine(path, course);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path =Path.Combine(path,customName+".json");
            return path;
        }

        public static List<T> LoadList<T>(string course=null, string customName=null)
        {
            var path = GetPath<T>(course, customName);
            if (!File.Exists(path)) return new List<T>();
            return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path));
        }

        public static T Load<T>(string course=null, string customName=null)
        {
            var path = GetPath<T>(course, customName);
            if (!File.Exists(path)) return default(T);
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

        }

        public static void SaveList<T>(List<T> list, string course=null,string customName=null)
        {
            var path = GetPath<T>(course, customName);
            File.WriteAllText(path, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        public static void Save<T>(T data, string course=null,string customName=null )
        {
            var path = GetPath<T>(course, customName);
            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        #endregion





        public static Section LoadCourseStructure(string CourseName)
        {
            return Load<Section>(CourseName, Env.StructureFileName);
        }

        public static void SaveCourseStructure(string CourseName,Section root)
        {
            Save(root,CourseName, Env.StructureFileName);

            var r = LoadList<VideoToCourse>();
            var section = r.Where(z => z.CourseGuid == root.Guid).FirstOrDefault();
            if (section == null)
                section = new VideoToCourse { CourseGuid = root.Guid };
            else
                r.Remove(section);
            section.VideoGuids.Clear();
            section.VideoGuids.AddRange(root.Items.OfType<Guid>());
            r.Add(section);

            SaveList(r);

        }


    }
}
