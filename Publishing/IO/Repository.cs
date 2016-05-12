using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing.IO
{
    public class Repository
    {
        readonly string Subfolder;
        public Repository(string subFolder)
        {
            this.Subfolder = subFolder;
        }
        public IOManager this[string index]
        {
            get
            {
                return new IOManager(Path.Combine(Subfolder, index));
            }
        }

    }
}
