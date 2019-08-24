using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class CollectionStatus
    {
        [JsonProperty("wish")]
        public int Wish { get; set; }

        [JsonProperty("collect")]
        public int Collect { get; set; }

        [JsonProperty("doing")]
        public int Doing { get; set; }

        [JsonProperty("on_hold")]
        public int OnHold { get; set; }

        [JsonProperty("dropped")]
        public int Dropped { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            CollectionStatus c = (CollectionStatus)obj;
            return Wish == c.Wish &&
                   Collect == c.Collect &&
                   Doing == c.Doing &&
                   OnHold == c.OnHold &&
                   Dropped == c.Dropped;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Wish + Collect + Doing + OnHold + Dropped;
        }
    }
}
