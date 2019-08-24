using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Avatar
    {
        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("small")]
        public string Small { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Avatar a = (Avatar)obj;
            return (Large == null ? Large == a.Large : Large.Equals(a.Large)) &&
                   (Medium == null ? Medium == a.Medium : Medium.Equals(a.Medium)) &&
                   (Small == null ? Small == a.Small : Small.Equals(a.Small));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Medium.Length;
        }
    }
}
