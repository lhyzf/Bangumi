using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Crt
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("role_name")]
        public string RoleName { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        [JsonProperty("actors")]
        public List<Actor> Actors { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Crt c = (Crt)obj;
            return Id == c.Id &&
                   (Url == null ? Url == c.Url : Url.Equals(c.Url)) &&
                   (Name == null ? Name == c.Name : Name.Equals(c.Name)) &&
                   (NameCn == null ? NameCn == c.NameCn : NameCn.Equals(c.NameCn)) &&
                   (RoleName == null ? RoleName == c.RoleName : RoleName.Equals(c.RoleName)) &&
                   (Images == null ? Images == c.Images : Images.Equals(c.Images)) &&
                   (Actors == null ? Actors == c.Actors : Actors.SequenceEqual(c.Actors));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
