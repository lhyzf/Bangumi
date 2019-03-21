using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Models
{
    public class CollectionStatus
    {
        public SubjectStatus status { get; set; }
        public int rating { get; set; }
        public string comment { get; set; }
        public List<string> tag { get; set; }
        public int ep_status { get; set; }
        public int lasttouch { get; set; }
        public string @private { get; set; }
        public User user { get; set; }
    }

    public class Avatar
    {
        public string large { get; set; }
        public string medium { get; set; }
        public string small { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public string url { get; set; }
        public string username { get; set; }
        public string nickname { get; set; }
        public Avatar avatar { get; set; }
        public string sign { get; set; }
        public int usergroup { get; set; }
    }

    public class SubjectStatus
    {
        public int id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
    }

    public class EpStatus
    {
        public int id { get; set; }
        public string css_name { get; set; }
        public string url_name { get; set; }
        public string cn_name { get; set; }
    }

    public class UserEp
    {
        public int id { get; set; }
        public EpStatus status { get; set; }
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
