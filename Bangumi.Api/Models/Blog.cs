using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 日志
    /// </summary>
    public class Blog
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// 概览
        /// </summary>
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [JsonPropertyName("image")]
        public string Image { get; set; }

        /// <summary>
        /// 回复数
        /// </summary>
        [JsonPropertyName("replies")]
        public int Replies { get; set; }

        /// <summary>
        /// 发布时间
        /// <br/>
        /// example: 1357144903
        /// </summary>
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }

        /// <summary>
        /// 发布时间
        /// <br/>
        /// example: 2013-1-2 16:41
        /// </summary>
        [JsonPropertyName("dateline")]
        public string DateLine { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Blog b = (Blog)obj;
            return Id == b.Id &&
                   Timestamp == b.Timestamp &&
                   Replies == b.Replies &&
                   Url.EqualsExT(b.Url) &&
                   Title.EqualsExT(b.Title) &&
                   Summary.EqualsExT(b.Summary) &&
                   Image.EqualsExT(b.Image) &&
                   DateLine.EqualsExT(b.DateLine) &&
                   User.EqualsExT(b.User);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
