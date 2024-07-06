using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 人物（基础模型）
    /// </summary>
    public class MonoBase
    {
        /// <summary>
        /// 人物 ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// 人物地址
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// large, medium, small, grid
        /// </summary>
        [JsonPropertyName("images")]
        public Images Images { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MonoBase c = (MonoBase)obj;
            return Id == c.Id &&
                   Url.EqualsExT(c.Url) &&
                   Name.EqualsExT(c.Name) &&
                   Images.EqualsExT(c.Images);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
