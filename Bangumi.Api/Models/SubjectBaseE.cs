using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目（基础模型）2
    /// </summary>
    public class SubjectBaseE
    {
        [JsonPropertyName("subject_id")]
        public int SubjectId { get; set; }

        [JsonPropertyName("subject")]
        public SubjectBase Subject { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectBaseE s = (SubjectBaseE)obj;
            return SubjectId == s.SubjectId &&
                   Subject.EqualsExT(s.Subject);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SubjectId;
        }
    }
}
