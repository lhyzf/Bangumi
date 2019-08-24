using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目状态
    /// </summary>
    public class SubjectStatus
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectStatus s = (SubjectStatus)obj;
            return Id == s.Id &&
                   (Type == null ? Type == s.Type : Type.Equals(s.Type)) &&
                   (Name == null ? Name == s.Name : Name.Equals(s.Name));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
