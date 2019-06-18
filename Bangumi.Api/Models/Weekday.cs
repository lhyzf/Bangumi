using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Weekday
    {
        [JsonProperty("en")]
        public string English { get; set; }

        [JsonProperty("cn")]
        public string Chinese { get; set; }

        [JsonProperty("ja")]
        public string Japanese { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
