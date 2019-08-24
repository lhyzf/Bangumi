using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 某一类别的收藏
    /// </summary>
    public class Collection2
    {
        public Collection2()
        {
            Name = string.Empty;
            NameCn = string.Empty;
            Collects = new List<Collection>();
        }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("collects")]
        public List<Collection> Collects { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Collection2 c = (Collection2)obj;
            return Type == c.Type &&
                   Name.Equals(c.Name) &&
                   NameCn.Equals(c.NameCn) &&
                   Collects.SequenceEqual(c.Collects);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Type;
        }
    }
}
