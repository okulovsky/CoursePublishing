using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursePublishing
{
    public class YoutubePlaylistEntry
    {
        public string Id { get; set; }


        public string VideoId { get; set; }

    }


    public class YoutubePlaylist
    {

        public string Channel { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public List<YoutubePlaylistEntry> Entries { get; }

        public YoutubePlaylist()
        {
            Entries = new List<YoutubePlaylistEntry>();
        }
    }
}
