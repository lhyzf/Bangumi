using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 用户正在观看
    /// </summary>
    public class Watching
    {
        public Watching()
        {
            Name = string.Empty;
            Subject = new Subject3();
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subject_id")]
        public int SubjectId { get; set; }

        [JsonProperty("ep_status")]
        public int EpStatus { get; set; }

        [JsonProperty("vol_status")]
        public int VolStatus { get; set; }

        [JsonProperty("lasttouch")]
        public int LastTouch { get; set; }

        [JsonProperty("subject")]
        public Subject3 Subject { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Watching w = (Watching)obj;
            return SubjectId == w.SubjectId &&
                   EpStatus == w.EpStatus &&
                   VolStatus == w.VolStatus &&
                   LastTouch == w.LastTouch &&
                   Name.Equals(w.Name) &&
                   Subject.Equals(w.Subject);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SubjectId;
        }
    }
}
