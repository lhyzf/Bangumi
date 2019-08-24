using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Api.Models
{
    public class AccessToken
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("expires")]
        public int Expires { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            AccessToken a = (AccessToken)obj;
            return ExpiresIn == a.ExpiresIn &&
                   Expires == a.Expires &&
                   (Token == null ? Token == a.Token : Token.Equals(a.Token)) &&
                   (TokenType == null ? TokenType == a.TokenType : TokenType.Equals(a.TokenType)) &&
                   (Scope == null ? Scope == a.Scope : Scope.Equals(a.Scope)) &&
                   (RefreshToken == null ? RefreshToken == a.RefreshToken : RefreshToken.Equals(a.RefreshToken)) &&
                   (UserId == null ? UserId == a.UserId : UserId.Equals(a.UserId));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Expires;
        }
    }
}
