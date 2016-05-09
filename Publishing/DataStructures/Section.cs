﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class SectionOrVideoGuid
    {
        public Section Section { get; set; }
        public Guid? VideoGuid { get; set; }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<Section> Sections(this IEnumerable<SectionOrVideoGuid> en)
        {
            return en.Where(z => z.Section != null).Select(z => z.Section);
        }
        public static IEnumerable<Guid> VideoGuids(this IEnumerable<SectionOrVideoGuid> en)
        {
            return en.Where(z => z.VideoGuid != null).Select(z => z.VideoGuid.Value);
        }
    }



    public class Section
    {
        public int Level { get; set; }
        public int Order { get; set; }
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
        public IEnumerable<SectionOrVideoGuid> Items
        {
            get
            {
                yield return new SectionOrVideoGuid { Section = this };
                foreach (var e in Sections)
                    foreach (var w in e.Items)
                        yield return w;
                foreach (var e in Videos)
                    yield return new SectionOrVideoGuid { VideoGuid = e };
            }
        }
        [JsonIgnore]
        public IEnumerable<Tuple<List<Tuple<Section, int>>, SectionOrVideoGuid>> ItemsWithPathes
        {
            get
            {
                yield return Tuple.Create(new List<Tuple<Section, int>>(), new SectionOrVideoGuid { Section = this });

                for (int i = 0; i < Sections.Count; i++)
                    foreach (var h in Sections[i].ItemsWithPathes)
                    {
                        h.Item1.Insert(0, Tuple.Create(this, i));
                        yield return h;
                    }

                for (int i = 0; i < Videos.Count; i++)
                    yield return Tuple.Create(new List<Tuple<Section, int>> { Tuple.Create(this, i) }, new SectionOrVideoGuid { VideoGuid = Videos[i] });
            }
        }
    }
}
