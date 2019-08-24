using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Weekday
    {
        public Weekday()
        {
            English = string.Empty;
            Chinese = string.Empty;
            Japanese = string.Empty;
        }

        [JsonProperty("en")]
        public string English { get; set; }

        [JsonProperty("cn")]
        public string Chinese { get; set; }

        [JsonProperty("ja")]
        public string Japanese { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Weekday w = (Weekday)obj;
            return Id == w.Id &&
                   English.Equals(w.English) &&
                   Chinese.Equals(w.Chinese) &&
                   Japanese.Equals(w.Japanese);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
