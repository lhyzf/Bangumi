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
    }
}
