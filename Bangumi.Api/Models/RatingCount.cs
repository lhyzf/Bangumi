using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class RatingCount
    {
        [JsonProperty("10")]
        public int _10 { get; set; }

        [JsonProperty("9")]
        public int _9 { get; set; }

        [JsonProperty("8")]
        public int _8 { get; set; }

        [JsonProperty("7")]
        public int _7 { get; set; }

        [JsonProperty("6")]
        public int _6 { get; set; }

        [JsonProperty("5")]
        public int _5 { get; set; }

        [JsonProperty("4")]
        public int _4 { get; set; }

        [JsonProperty("3")]
        public int _3 { get; set; }

        [JsonProperty("2")]
        public int _2 { get; set; }

        [JsonProperty("1")]
        public int _1 { get; set; }
    }
}
