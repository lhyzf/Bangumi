using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Blog
    {
        public Blog()
        {
            Url = string.Empty;
            Title = string.Empty;
            Summary = string.Empty;
            Image = string.Empty;
            DateLine = string.Empty;
            User = new User();
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("replies")]
        public int Replies { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("dateline")]
        public string DateLine { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Blog b = (Blog)obj;
            return Id == b.Id &&
                   Timestamp == b.Timestamp &&
                   Replies == b.Replies &&
                   Url.Equals(b.Url) &&
                   Title.Equals(b.Title) &&
                   Summary.Equals(b.Summary) &&
                   Image.Equals(b.Image) &&
                   DateLine.Equals(b.DateLine) &&
                   User.Equals(b.User);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
