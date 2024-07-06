using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 讨论版
    /// </summary>
    public class Topic
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
        /// 所属对象（条目） ID
        /// </summary>
        [JsonPropertyName("main_id")]
        public int MainId { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }

        /// <summary>
        /// 最后回复时间
        /// </summary>
        [JsonPropertyName("lastpost")]
        public int LastPost { get; set; }

        /// <summary>
        /// 回复数
        /// </summary>
        [JsonPropertyName("replies")]
        public int Replies { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Topic t = (Topic)obj;
            return Id == t.Id &&
                   MainId == t.MainId &&
                   Timestamp == t.Timestamp &&
                   LastPost == t.LastPost &&
                   Replies == t.Replies &&
                   Url.EqualsExT(t.Url) &&
                   Title.EqualsExT(t.Title) &&
                   User.EqualsExT(t.User);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
