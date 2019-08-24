﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("nickname")]
        public string NickName { get; set; }

        [JsonProperty("avatar")]
        public Avatar Avatar { get; set; }

        [JsonProperty("sign")]
        public string Sign { get; set; }

        [JsonProperty("usergroup")]
        public int UserGroup { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            User u = (User)obj;
            return Id == u.Id &&
                   UserGroup == u.UserGroup &&
                   (Url == null ? Url == u.Url : Url.Equals(u.Url)) &&
                   (UserName == null ? UserName == u.UserName : UserName.Equals(u.UserName)) &&
                   (NickName == null ? NickName == u.NickName : NickName.Equals(u.NickName)) &&
                   (Sign == null ? Sign == u.Sign : Sign.Equals(u.Sign)) &&
                   (Avatar == null ? Avatar == u.Avatar : Avatar.Equals(u.Avatar));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
