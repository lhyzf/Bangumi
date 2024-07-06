using Bangumi.Api.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目的收藏信息
    /// </summary>
    public class CollectionStatusE
    {
        [JsonPropertyName("status")]
        public CollectionStatus Status { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [JsonPropertyName("tag")]
        public List<string> Tags { get; set; }

        /// <summary>
        /// 章节观看状态
        /// </summary>
        [JsonPropertyName("ep_status")]
        public int EpStatus { get; set; }

        /// <summary>
        /// 书籍章节状态
        /// </summary>
        [JsonPropertyName("vol_status")]
        public int VolStatus { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [JsonPropertyName("lasttouch")]
        public long LastTouch { get; set; }

        /// <summary>
        /// 仅自己可见
        /// </summary>
        [JsonPropertyName("private")]
        public int Private { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            CollectionStatusE s = (CollectionStatusE)obj;
            return Rating == s.Rating &&
                   LastTouch == s.LastTouch &&
                   EpStatus == s.EpStatus &&
                   VolStatus == s.VolStatus &&
                   Private == s.Private &&
                   Comment.EqualsExT(s.Comment) &&
                   Status.EqualsExT(s.Status) &&
                   User.EqualsExT(s.User) &&
                   Tags.SequenceEqualExT(s.Tags);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (int)LastTouch;
        }
    }
}
