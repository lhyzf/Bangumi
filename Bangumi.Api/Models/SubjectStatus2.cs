using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 收藏的条目的状态信息
    /// </summary>
    public class SubjectStatus2
    {
        [JsonProperty("status")]
        public SubjectStatus Status { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("tag")]
        public List<string> Tags { get; set; }

        [JsonProperty("ep_status")]
        public int EpStatus { get; set; }

        [JsonProperty("lasttouch")]
        public int LastTouch { get; set; }

        [JsonProperty("private")]
        public string Private { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }
}
