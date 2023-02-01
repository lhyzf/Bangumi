﻿using Bangumi.Api.Common;
using Newtonsoft.Json;
using System;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 简略条目详情，适用于Watching
    /// </summary>
    public class SubjectForWatching : SubjectBase
    {
        /// <summary>
        /// 话数
        /// </summary>
        [JsonProperty("eps")]
        [Obsolete]
        public int Eps { get; set; }

        /// <summary>
        /// 话数
        /// </summary>
        [JsonProperty("eps_count")]
        public int? EpsCount { get; set; }

        /// <summary>
        /// 卷数 - 书籍
        /// </summary>
        [JsonProperty("vols_count")]
        public int? VolsCount { get; set; }

        /// <summary>
        /// doing
        /// </summary>
        [JsonProperty("collection")]
        public SubjectCollection Collection { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SubjectForWatching s = (SubjectForWatching)obj;
            return base.Equals(obj) &&
                   Eps == s.Eps &&
                   EpsCount == s.EpsCount &&
                   VolsCount == s.VolsCount &&
                   Collection.EqualsExT(s.Collection);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
