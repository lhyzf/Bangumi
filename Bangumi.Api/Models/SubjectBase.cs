﻿using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目（基础模型）
    /// </summary>
    public class SubjectBase
    {
        /// <summary>
        /// 条目 ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// 条目地址
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("type")]
        public SubjectType Type { get; set; }

        /// <summary>
        /// 条目名称
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 条目中文名称
        /// </summary>
        [JsonPropertyName("name_cn")]
        public string NameCn { get; set; }

        /// <summary>
        /// 剧情简介
        /// </summary>
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// 放送开始日期
        /// </summary>
        [JsonPropertyName("air_date")]
        public string AirDate { get; set; }

        /// <summary>
        /// 放送星期
        /// </summary>
        [JsonPropertyName("air_weekday")]
        public int AirWeekday { get; set; }

        /// <summary>
        /// large, common, medium, small, grid
        /// </summary>
        [JsonPropertyName("images")]
        public Images Images { get; set; }

        public string AirWeekdayCn => AirWeekday switch
        {
            1 => "星期一",
            2 => "星期二",
            3 => "星期三",
            4 => "星期四",
            5 => "星期五",
            6 => "星期六",
            7 => "星期日",
            _ => string.Empty,
        };

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectBase s = (SubjectBase)obj;
            return Id == s.Id &&
                   Type == s.Type &&
                   AirWeekday == s.AirWeekday &&
                   Url.EqualsExT(s.Url) &&
                   Name.EqualsExT(s.Name) &&
                   NameCn.EqualsExT(s.NameCn) &&
                   Summary.EqualsExT(s.Summary) &&
                   AirDate.EqualsExT(s.AirDate) &&
                   Images.EqualsExT(s.Images);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
