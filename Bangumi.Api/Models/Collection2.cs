using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 某一类别的收藏
    /// </summary>
    public class Collection2
    {
        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("collects")]
        public List<Collection> Collects { get; set; }
    }
}
