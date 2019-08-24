using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 某一类别的收藏详细列表
    /// </summary>
    public class Collection
    {
        public Collection()
        {
            Status = new SubjectStatus();
            Items = new List<Subject2>();
        }

        [JsonProperty("status")]
        public SubjectStatus Status { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("list")]
        public List<Subject2> Items { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Collection c = (Collection)obj;
            return Count == c.Count &&
                   Status.Equals(c.Status) &&
                   Items.SequenceEqual(c.Items);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Count;
        }
    }
}
