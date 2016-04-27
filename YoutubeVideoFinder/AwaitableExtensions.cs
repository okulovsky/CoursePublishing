using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSync
{
    public static class AwaitableExtensions
    {
        public static T RunSync<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
