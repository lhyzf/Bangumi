using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 收藏人数
    /// </summary>
    public class SubjectCollection
    {
        /// <summary>
        /// 想做
        /// </summary>
        [JsonPropertyName("wish")]
        public int Wish { get; set; }

        /// <summary>
        /// 做过
        /// </summary>
        [JsonPropertyName("collect")]
        public int Collect { get; set; }

        /// <summary>
        /// 在做
        /// </summary>
        [JsonPropertyName("doing")]
        public int Doing { get; set; }

        /// <summary>
        /// 搁置
        /// </summary>
        [JsonPropertyName("on_hold")]
        public int OnHold { get; set; }

        /// <summary>
        /// 抛弃
        /// </summary>
        [JsonPropertyName("dropped")]
        public int Dropped { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectCollection c = (SubjectCollection)obj;
            return Wish == c.Wish &&
                   Collect == c.Collect &&
                   Doing == c.Doing &&
                   OnHold == c.OnHold &&
                   Dropped == c.Dropped;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Wish + Collect + Doing + OnHold + Dropped;
        }
    }
}
