using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Models
{
    public class Collection
    {
        public int doing { get; set; }
    }

    public class Subject
    {
        public int id { get; set; }
        public string url { get; set; }
        public int type { get; set; }
        public string name { get; set; }
        public string name_cn { get; set; }
        public string summary { get; set; }
        public int eps { get; set; }
        public int eps_count { get; set; }
        public string air_date { get; set; }
        public int air_weekday { get; set; }
        public Images images { get; set; }
        public Collection collection { get; set; }
    }

    public class Watching
    {
        public string name { get; set; }
        public int subject_id { get; set; }
        public int ep_status { get; set; }
        public int vol_status { get; set; }
        public int lasttouch { get; set; }
        public Subject subject { get; set; }
    }
}
