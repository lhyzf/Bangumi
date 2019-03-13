using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Models
{
    public class Weekday
    {
        public string en { get; set; }
        public string cn { get; set; }
        public string ja { get; set; }
        public int id { get; set; }
    }

    public class BangumiTimeLine
    {
        public Weekday weekday { get; set; }
        public List<Subject> items { get; set; }
    }
}
