using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 评分
    /// </summary>
    public class Rating
    {
        /// <summary>
        /// 总评分人数
        /// </summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("count")]
        public RatingCount Count { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        [JsonPropertyName("score")]
        public double Score { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Rating r = (Rating)obj;
            return Total == r.Total &&
                   Score == r.Score &&
                   Count.EqualsExT(r.Count);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Total;
        }
    }
}
