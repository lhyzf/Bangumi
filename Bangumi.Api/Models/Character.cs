﻿using Bangumi.Api.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 虚拟角色
    /// </summary>
    public class Character : Mono
    {
        [JsonPropertyName("actors")]
        public List<Actor> Actors { get; set; }

        /// <summary>
        /// 角色类型
        /// <br/>example: 主角
        /// </summary>
        [JsonPropertyName("role_name")]
        public string RoleName { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Character c = (Character)obj;
            return base.Equals(obj) &&
                   RoleName.EqualsExT(c.RoleName) &&
                   Actors.SequenceEqualExT(c.Actors);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
