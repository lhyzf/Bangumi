using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class BangumiTimeLine
    {
        [JsonProperty("weekday")]
        public Weekday Weekday { get; set; }

        [JsonProperty("items")]
        public List<Subject> Items { get; set; }
    }
}
