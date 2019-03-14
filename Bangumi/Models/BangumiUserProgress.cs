using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Models
{
    public class Status
    {
        public int id { get; set; }
        public string css_name { get; set; }
        public string url_name { get; set; }
        public string cn_name { get; set; }
    }

    public class UserEp
    {
        public int id { get; set; }
        public Status status { get; set; }
    }

    public class Progress
    {
        public int subject_id { get; set; }
        public List<UserEp> eps { get; set; }
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
