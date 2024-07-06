using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 各分值评分人数
    /// </summary>
    public class RatingCount
    {
        [JsonPropertyName("10")]
        public int _10 { get; set; }

        [JsonPropertyName("9")]
        public int _9 { get; set; }

        [JsonPropertyName("8")]
        public int _8 { get; set; }

        [JsonPropertyName("7")]
        public int _7 { get; set; }

        [JsonPropertyName("6")]
        public int _6 { get; set; }

        [JsonPropertyName("5")]
        public int _5 { get; set; }

        [JsonPropertyName("4")]
        public int _4 { get; set; }

        [JsonPropertyName("3")]
        public int _3 { get; set; }

        [JsonPropertyName("2")]
        public int _2 { get; set; }

        [JsonPropertyName("1")]
        public int _1 { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            RatingCount r = (RatingCount)obj;
            return _1 == r._1 &&
                   _2 == r._2 &&
                   _3 == r._3 &&
                   _4 == r._4 &&
                   _5 == r._5 &&
                   _6 == r._6 &&
                   _7 == r._7 &&
                   _8 == r._8 &&
                   _9 == r._9 &&
                   _10 == r._10;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (_1 + _2 + _3 + _4 + _5 + _6 + _7 + _8 + _9 + _10) / 10;
        }
    }
}
