﻿using System.Text.Json.Serialization;

namespace Bangumi.Data.Models
{

    public class SiteMeta
    {

        /// <summary>
        /// Examples: "番组计划",AcFun","哔哩哔哩","动漫花园"
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Examples: "http://bangumi.tv/subject/{{id}}","http://www.acfun.cn/v/ab{{id}}",
        /// "https://bangumi.bilibili.com/anime/{{id}}","https://share.dmhy.org/topics/list?keyword={{id}}"
        /// </summary>
        [JsonPropertyName("urlTemplate")]
        public string UrlTemplate { get; set; }

        /// <summary>
        /// Examples: "resource","info","onair"
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

}
