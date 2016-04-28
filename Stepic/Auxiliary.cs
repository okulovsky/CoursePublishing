using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stepic
{
    static class MyExtension
    {
        public static IEnumerable<T> Now<T>(this Task<T> task)
        {
            task.Wait();
            yield return task.Result;
        }

        public static void PrintJson<T>(this IEnumerable<T> o)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(o.First(), Newtonsoft.Json.Formatting.Indented));
        }


    }

}
