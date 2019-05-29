using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Models
{
    public class Rating
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("count")]
        public RatingCount Count { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }
    }
}
