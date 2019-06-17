using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Models
{
    /// <summary>
    /// 用户正在观看
    /// </summary>
    public class Watching
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subject_id")]
        public int SubjectId { get; set; }

        [JsonProperty("ep_status")]
        public int EpStatus { get; set; }

        [JsonProperty("vol_status")]
        public int VolStatus { get; set; }

        [JsonProperty("lasttouch")]
        public int LastTouch { get; set; }

        [JsonProperty("subject")]
        public Subject3 Subject { get; set; }
    }
}
