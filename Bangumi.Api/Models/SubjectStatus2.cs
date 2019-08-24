using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 收藏的条目的状态信息
    /// </summary>
    public class SubjectStatus2
    {
        [JsonProperty("status")]
        public SubjectStatus Status { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("tag")]
        public List<string> Tags { get; set; }

        [JsonProperty("ep_status")]
        public int EpStatus { get; set; }

        [JsonProperty("lasttouch")]
        public int LastTouch { get; set; }

        [JsonProperty("private")]
        public string Private { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectStatus2 s = (SubjectStatus2)obj;
            return Rating == s.Rating &&
                   EpStatus == s.EpStatus &&
                   LastTouch == s.LastTouch &&
                   (Comment == null ? Comment == s.Comment : Comment.Equals(s.Comment)) &&
                   (Private == null ? Private == s.Private : Private.Equals(s.Private)) &&
                   (Status == null ? Status == s.Status : Status.Equals(s.Status)) &&
                   (User == null ? User == s.User : User.Equals(s.User)) &&
                   (Tags == null ? Tags == s.Tags : Tags.SequenceEqual(s.Tags));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return LastTouch;
        }
    }
}
