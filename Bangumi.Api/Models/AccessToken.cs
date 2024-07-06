using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    public class AccessToken
    {
        [JsonPropertyName("access_token")]
        public string Token { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("expires")]
        public int Expires { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("user_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int UserId { get; set; }

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
                   UserId == a.UserId &&
                   Token.EqualsExT(a.Token) &&
                   TokenType.EqualsExT(a.TokenType) &&
                   Scope.EqualsExT(a.Scope) &&
                   RefreshToken.EqualsExT(a.RefreshToken);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Expires;
        }
    }
}
