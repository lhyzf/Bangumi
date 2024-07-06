using Bangumi.Api.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目详情
    /// </summary>
    public class SubjectLarge : SubjectBase
    {
        /// <summary>
        /// 话数
        /// </summary>
        [JsonPropertyName("eps_count")]
        public int? EpsCount { get; set; }

        /// <summary>
        /// 卷数 - 书籍
        /// </summary>
        [JsonPropertyName("vols_count")]
        public int? VolsCount { get; set; }

        /// <summary>
        /// 排名
        /// </summary>
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("rating")]
        public Rating Rating { get; set; }

        [JsonPropertyName("collection")]
        public SubjectCollection Collection { get; set; }

        /// <summary>
        /// 章节
        /// </summary>
        [JsonPropertyName("eps")]
        public List<Episode> Eps { get; set; }

        /// <summary>
        /// 角色信息
        /// </summary>
        [JsonPropertyName("crt")]
        public List<Character> Characters { get; set; }

        /// <summary>
        /// 制作人员信息
        /// </summary>
        [JsonPropertyName("staff")]
        public List<Person> Staff { get; set; }

        [JsonPropertyName("topic")]
        public List<Topic> Topics { get; set; }

        [JsonPropertyName("blog")]
        public List<Blog> Blogs { get; set; }


        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectLarge s = (SubjectLarge)obj;
            return base.Equals(obj) &&
                   Rank == s.Rank &&
                   EpsCount == s.EpsCount &&
                   VolsCount == s.VolsCount &&
                   Rating.EqualsExT(s.Rating) &&
                   Collection.EqualsExT(s.Collection) &&
                   Eps.SequenceEqualExT(s.Eps) &&
                   Characters.SequenceEqualExT(s.Characters) &&
                   Staff.SequenceEqualExT(s.Staff) &&
                   Topics.SequenceEqualExT(s.Topics) &&
                   Blogs.SequenceEqualExT(s.Blogs);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
