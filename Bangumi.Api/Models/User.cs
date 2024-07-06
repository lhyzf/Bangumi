using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户 id
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// 用户主页地址
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [JsonPropertyName("nickname")]
        public string NickName { get; set; }

        /// <summary>
        /// large, medium, small
        /// </summary>
        [JsonPropertyName("avatar")]
        public Images Avatar { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        [JsonPropertyName("sign")]
        public string Sign { get; set; }

        [JsonPropertyName("usergroup")]
        public UserGroup UserGroup { get; set; }

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
                   Url.EqualsExT(u.Url) &&
                   UserName.EqualsExT(u.UserName) &&
                   NickName.EqualsExT(u.NickName) &&
                   Sign.EqualsExT(u.Sign) &&
                   Avatar.EqualsExT(u.Avatar);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
