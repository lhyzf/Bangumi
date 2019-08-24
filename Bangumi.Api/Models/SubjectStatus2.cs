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
        public SubjectStatus2()
        {
            Comment = string.Empty;
            Private = string.Empty;
            Status = new SubjectStatus();
            User = new User();
            Tags = new List<string>();
        }

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
                   Comment.Equals(s.Comment) &&
                   Private.Equals(s.Private) &&
                   Status.Equals(s.Status) &&
                   User.Equals(s.User) &&
                   Tags.SequenceEqual(s.Tags);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return LastTouch;
        }
    }
}
