﻿using System;
using System.Text.Json.Serialization;

namespace Bangumi.Data.Models
{

    public class Site
    {

        /// <summary>
        /// Examples: "bangumi", "iqiyi", "nicovideo", "bilibili", "pptv"
        /// </summary>
        [JsonPropertyName("site")]
        public string SiteName { get; set; }

        /// <summary>
        /// Examples: "213759", "a_19rrhb12tt", "40379", "41983", "kyojin-no-hoshi"
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Examples: "2015-10-29T08:31:41Z", "2012-12-24T06:00:00Z", "1969-10-04T16:00:00Z", "1969-10-04T16:00:01Z", "2016-12-05T06:24:50Z"
        /// </summary>
        [JsonPropertyName("begin")]
        public DateTimeOffset? Begin { get; set; }

        /// <summary>
        /// Examples: <br/>
        /// 一次性："R/2020-01-01T13:00:00Z/P0D", <br/>
        /// 周播："R/2020-01-01T13:00:00Z/P7D", <br/>
        /// 日播："R/2020-01-01T13:00:00Z/P1D", <br/>
        /// 月播："R/2020-01-01T13:00:00Z/P1M"
        /// </summary>
        [JsonPropertyName("broadcast")]
        public string Broadcast { get; set; }

        /// <summary>
        /// Examples: "http://www.iqiyi.com/dongman/20130715/839f4bf2c2cbe419.html", "http://www.iqiyi.com/dongman/20130715/d7a321d48f825733.html", "http://www.iqiyi.com/v_19rrifx03h.html", "http://www.iqiyi.com/v_19rrifx03f.html", "http://www.iqiyi.com/dongman/20130715/75c20dc1fe211093.html"
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Examples: "", "港区可见", "分割为 TV 放送", "原始版本为剧场版但是 Netflix 目前将其做成 14 集 TV 分割形式放送"
        /// </summary>
        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        public Site Clone() => new Site
        {
            SiteName = SiteName,
            Id = Id,
            Begin = Begin,
            Broadcast = Broadcast,
            Url = Url,
            Comment = Comment
        };
    }

}
