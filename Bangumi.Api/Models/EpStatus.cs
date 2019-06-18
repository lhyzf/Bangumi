using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 章节状态
    /// </summary>
    public class EpStatus
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("css_name")]
        public string CssName { get; set; }

        [JsonProperty("url_name")]
        public string UrlName { get; set; }

        [JsonProperty("cn_name")]
        public string CnName { get; set; }
    }
}
