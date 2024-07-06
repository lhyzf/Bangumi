using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 封面、肖像
    /// </summary>
    public class Images
    {
        [JsonPropertyName("large")]
        public string Large { get; set; }

        [JsonPropertyName("common")]
        public string Common { get; set; }

        [JsonPropertyName("medium")]
        public string Medium { get; set; }

        [JsonPropertyName("small")]
        public string Small { get; set; }

        [JsonPropertyName("grid")]
        public string Grid { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Images i = (Images)obj;
            return Large.EqualsExT(i.Large) &&
                   Common.EqualsExT(i.Common) &&
                   Medium.EqualsExT(i.Medium) &&
                   Small.EqualsExT(i.Small) &&
                   Grid.EqualsExT(i.Grid);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Common.Length;
        }
    }
}
