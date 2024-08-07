﻿using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Bangumi.Data.Models
{

    public class BangumiDataSet
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            Converters =
            {
                new JsonConverters.CustomDateTimeOffsetConverter()
            }
        };

        /// <summary>
        /// Examples: {"bangumi":{"title":"番组计划","urlTemplate":"http://bangumi.tv/subject/{{id}}","type":"info"},"saraba1st":{"title":"Stage1st","urlTemplate":"https://bbs.saraba1st.com/2b/thread-{{id}}-1-1.html","type":"info"},"acfun":{"title":"AcFun","urlTemplate":"http://www.acfun.cn/v/ab{{id}}","type":"onair"},"bilibili":{"title":"哔哩哔哩","urlTemplate":"https://bangumi.bilibili.com/anime/{{id}}","type":"onair"},"tucao":{"title":"TUCAO","urlTemplate":"http://www.tucao.tv/index.php?m=search&c=index&a=init2&q={{id}}","type":"onair"},"sohu":{"title":"搜狐视频","urlTemplate":"https://tv.sohu.com/{{id}}","type":"onair"},"youku":{"title":"优酷","urlTemplate":"https://list.youku.com/show/id_z{{id}}.html","type":"onair"},"tudou":{"title":"土豆","urlTemplate":"https://www.tudou.com/albumcover/{{id}}.html","type":"onair"},"qq":{"title":"腾讯视频","urlTemplate":"https://v.qq.com/detail/{{id}}.html","type":"onair"},"iqiyi":{"title":"爱奇艺","urlTemplate":"https://www.iqiyi.com/{{id}}.html","type":"onair"},"letv":{"title":"乐视","urlTemplate":"https://www.le.com/comic/{{id}}.html","type":"onair"},"pptv":{"title":"PPTV","urlTemplate":"http://v.pptv.com/page/{{id}}.html","type":"onair"},"kankan":{"title":"响巢看看","urlTemplate":"http://movie.kankan.com/movie/{{id}}","type":"onair"},"mgtv":{"title":"芒果tv","urlTemplate":"https://www.mgtv.com/h/{{id}}.html","type":"onair"},"nicovideo":{"title":"Niconico","urlTemplate":"https://ch.nicovideo.jp/{{id}}","type":"onair"},"netflix":{"title":"Netflix","urlTemplate":"https://www.netflix.com/title/{{id}}","type":"onair"},"dmhy":{"title":"动漫花园","urlTemplate":"https://share.dmhy.org/topics/list?keyword={{id}}","type":"resource"},"nyaa":{"title":"nyaa","urlTemplate":"https://www.nyaa.se/?page=search&term={{id}}","type":"resource"}}
        /// </summary>
        [JsonPropertyName("siteMeta")]
        public IDictionary<string, SiteMeta> SiteMeta { get; set; }

        /// <summary>
        /// Examples: [{"title":"新しい動画 3つのはなし","titleTranslate":{"zh-Hans":["新动画 三个故事"],"en":["Three Tales"]},"type":"tv","lang":"ja","officialSite":"","begin":"1960-01-15T16:00:00Z","end":"1960-01-15T16:30:00Z","comment":"","sites":[{"site":"bangumi","id":"213759"}]},{"title":"ゲゲゲの鬼太郎","titleTranslate":{"zh-Hans":["鬼太郎","咯咯咯鬼太郎 1"]},"type":"tv","lang":"ja","officialSite":"","begin":"1968-01-03T16:00:00Z","end":"1969-03-30T16:30:00Z","comment":"","sites":[{"site":"iqiyi","id":"a_19rrhb12tt","begin":"2015-10-29T08:31:41Z","official":true,"premuiumOnly":false,"censored":null,"exist":true,"comment":""},{"site":"bangumi","id":"40379"}]},{"title":"巨人の星","titleTranslate":{"zh-Hans":["巨人之星"]},"type":"tv","lang":"ja","officialSite":"","begin":"1968-03-30T16:00:00Z","end":"1971-09-18T16:30:00Z","comment":"","sites":[{"site":"bangumi","id":"41983"},{"site":"nicovideo","id":"kyojin-no-hoshi","begin":"2012-12-24T06:00:00Z","official":true,"premuiumOnly":true,"censored":null,"exist":true,"comment":""}]},{"title":"ゲゲゲの鬼太郎","titleTranslate":{"zh-Hans":["鬼太郎"]},"type":"movie","lang":"ja","officialSite":"","begin":"1968-07-21T16:00:00Z","end":"1968-07-21T17:00:00Z","comment":"","sites":[{"site":"bangumi","id":"211091"}]},{"title":"サスケ","titleTranslate":{"zh-Hans":["死神少年","佐助"]},"type":"tv","lang":"ja","officialSite":"","begin":"1968-09-03T13:15:00Z","end":"1969-03-25T13:45:00Z","comment":"","sites":[{"site":"bangumi","id":"150325"}]},{"title":"妖怪人間ベム","titleTranslate":{"zh-Hans":["妖怪人间贝姆"]},"type":"tv","lang":"ja","officialSite":"","begin":"1968-10-07T07:00:00Z","end":"1969-03-31T07:24:00Z","comment":"","sites":[{"site":"bangumi","id":"53743"}]}]
        /// </summary>
        [JsonPropertyName("items")]
        public IList<Item> Items { get; set; }

        public static BangumiDataSet FromJson(string json) =>
            JsonSerializer.Deserialize<BangumiDataSet>(json, SerializerOptions);
    }

}
