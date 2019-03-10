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

    public class Ep
    {
        public int id { get; set; }
        public Status status { get; set; }
    }

    public class Progress
    {
        public int subject_id { get; set; }
        public List<Ep> eps { get; set; }
    }
}
