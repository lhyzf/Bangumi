﻿using Bangumi.Api.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 某一条目用户收视进度
    /// </summary>
    public class Progress
    {
        [JsonPropertyName("subject_id")]
        public int SubjectId { get; set; }

        [JsonPropertyName("eps")]
        public List<EpStatusE> Eps { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Progress p = (Progress)obj;
            return SubjectId == p.SubjectId &&
                   Eps.SequenceEqualExT(p.Eps);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SubjectId;
        }
    }
}
