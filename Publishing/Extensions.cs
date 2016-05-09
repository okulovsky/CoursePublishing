using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public static class EnumerableExtensions1
    {
        public static IEnumerable<Tuple<T,T>> Pairs<T>(this IEnumerable<T> en)
        {
            bool first = true;
            T element = default(T);
            foreach(var e in en)
            {
                if (!first)
                {
                    yield return Tuple.Create(element, e);
                }
                element = e;
                first = false;
            }
        }

        public static T RunSync<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
