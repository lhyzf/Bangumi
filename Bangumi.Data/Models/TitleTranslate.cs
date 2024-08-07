﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bangumi.Data.Models
{

    public class TitleTranslate
    {

        /// <summary>
        /// Examples: ["新动画 三个故事"], ["鬼太郎","咯咯咯鬼太郎 1"], ["巨人之星"], ["鬼太郎"], ["死神少年","佐助"]
        /// </summary>
        [JsonPropertyName("zh-Hans")]
        public IList<string> ZhHans { get; set; }

        /// <summary>
        /// Examples: ["Three Tales"], ["Triton of the Sea"], ["DEVILMAN"], ["Yatterman"], ["Treasure Island"]
        /// </summary>
        [JsonPropertyName("en")]
        public IList<string> En { get; set; }

        /// <summary>
        /// Examples: ["寵物小精靈","神奇寶貝"], ["滿腦都是○○的我沒辦法談戀愛"]
        /// </summary>
        [JsonPropertyName("zh-Hant")]
        public IList<string> ZhHant { get; set; }

        /// <summary>
        /// Examples: ["霊剣山 星屑たちの宴"], ["一人之下 the outcast"], ["チーティングクラフト"], ["Bloodivores"], ["銀の墓守り"]
        /// </summary>
        [JsonPropertyName("ja")]
        public IList<string> Ja { get; set; }
    }

}
