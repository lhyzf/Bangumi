using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Avatar
    {
        public Avatar()
        {
            Large = string.Empty;
            Medium = string.Empty;
            Small = string.Empty;
        }

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
            return Large.Equals(a.Large) &&
                   Medium.Equals(a.Medium) &&
                   Small.Equals(a.Small);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Medium.Length;
        }
    }
}
