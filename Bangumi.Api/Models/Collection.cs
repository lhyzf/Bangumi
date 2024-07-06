using Bangumi.Api.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 某一类别的收藏详细列表
    /// </summary>
    public class Collection
    {
        [JsonPropertyName("status")]
        public CollectionStatus Status { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("list")]
        public List<SubjectBaseE> Items { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Collection c = (Collection)obj;
            return Count == c.Count &&
                   Status.EqualsExT(c.Status) &&
                   Items.SequenceEqualExT(c.Items);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Count;
        }
    }
}
