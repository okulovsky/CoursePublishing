using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoursePublishing.IO
{
    public class IOManager
    {
        readonly string Subfolder;
        public IOManager(string subFolder=null)
        {
            Subfolder = subFolder;
        }

        public string GetFileName(string filename)
        {
            var path = Env.DataFolder;
            if (Subfolder != null)
                path = Path.Combine(path, Subfolder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, filename);
            return path;
        }

        string GetPath<T>()
        {
            var customName = typeof(T).Name;
            return GetFileName(customName + ".json");
        }

        public List<T> LoadList<T>()
        {
            var path = GetPath<T>();
            if (!File.Exists(path)) return new List<T>();
            return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path));
        }

        public T Load<T>()
        {
            var path = GetPath<T>();
            if (!File.Exists(path)) return default(T);
            var text = File.ReadAllText(path);
            var obj = JsonConvert.DeserializeObject<T>(text);
            return obj;

        }

        public void SaveList<T>(List<T> list)
        {
            var path = GetPath<T>();
            File.WriteAllText(path, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        public void Save<T>(T data)
        {
            var path = GetPath<T>();
            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public void UpdateList<T>(List<T> data, Func<T, string> keySelection)
        {
            var existing = LoadList<T>();
            var replacements = data.ToDictionary(z => keySelection(z), z => z);
            for (int i = 0; i < existing.Count; i++)
            {
                var key = keySelection(existing[i]);
                if (replacements.ContainsKey(key))
                {
                    existing[i] = replacements[key];
                    replacements.Remove(key);
                }
            }
            existing.AddRange(replacements.Values);
            SaveList(existing);
        }

        public T LoadOrInit<T>()
            where T : new()
        {
            var path = GetPath<T>();
            if (!File.Exists(path))
            {
                var t = new T();
                Save(t);
                Process.Start(path).WaitForExit();
            }
            return Load<T>();
        }

        public T LoadInitOrEdit<T>(int count = 9)
            where T : new()
        {
            var path = GetPath<T>();
            if (!File.Exists(path)) return LoadOrInit<T>();
            Console.Write($"Press any key to edit settings {typeof(T).Name} in  ");
            for (int i = count; i >= 0; i--)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) break;
                    try
                    {
                        Save(Load<T>());
                    }
                    catch { }
                    Process.Start(path).WaitForExit();
                    break;
                }
                Console.Write($"\b{i + 1}");
                Thread.Sleep(1000);
            }
            Console.WriteLine();
            return Load<T>();
        }
    }
}
