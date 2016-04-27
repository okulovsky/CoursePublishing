using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class Section
    {
        public int Level { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public List<Guid> Videos { get; private set; }
        public List<Section> Sections { get; private set; }

        public Section()
        {
            Videos = new List<Guid>();
            Sections = new List<Section>();
        }

        [JsonIgnore]
        public IEnumerable<object> Items
        {
            get 
            {
                yield return this;
                foreach(var e in Sections)
                    foreach(var w in e.Items)
                         yield return w;
                foreach (var e in Videos)
                    yield return e;
            }
        }
    }
}
