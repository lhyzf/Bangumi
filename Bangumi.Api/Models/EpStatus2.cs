using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class EpStatus2
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public EpStatus Status { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EpStatus2 e = (EpStatus2)obj;
            return Id == e.Id &&
                   (Status == null ? Status == e.Status : Status.Equals(e.Status));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
