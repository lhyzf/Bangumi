using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 某一类别的收藏详细列表
    /// </summary>
    public class Collection
    {
        [JsonProperty("status")]
        public SubjectStatus Status { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("list")]
        public List<Subject2> Items { get; set; }
    }
}
