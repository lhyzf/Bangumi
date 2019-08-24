using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Actor
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Actor a = (Actor)obj;
            return Id == a.Id &&
                   (Url == null ? Url == a.Url : Url.Equals(a.Url)) &&
                   (Name == null ? Name == a.Name : Name.Equals(a.Name)) &&
                   (Images == null ? Images == a.Images : Images.Equals(a.Images));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
