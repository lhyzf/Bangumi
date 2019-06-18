using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目状态
    /// </summary>
    public class SubjectStatus
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
