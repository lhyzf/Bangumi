using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 条目详情
    /// </summary>
    public class Subject
    {
        public Subject()
        {
            Url = string.Empty;
            Name = string.Empty;
            NameCn = string.Empty;
            Summary = string.Empty;
            AirDate = string.Empty;
            Rating = new Rating();
            Images = new Images();
            Collection = new CollectionStatus();
            Eps = new List<Ep>();
            Characters = new List<Crt>();
            Staff = new List<Staff>();
            Topics = new List<Topic>();
            Blogs = new List<Blog>();
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_cn")]
        public string NameCn { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("eps_count")]
        public int EpsCount { get; set; }

        [JsonProperty("air_date")]
        public string AirDate { get; set; }

        [JsonProperty("air_weekday")]
        public int AirWeekday { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        [JsonProperty("collection")]
        public CollectionStatus Collection { get; set; }

        [JsonProperty("eps")]
        public List<Ep> Eps { get; set; }

        [JsonProperty("crt")]
        public List<Crt> Characters { get; set; }

        [JsonProperty("staff")]
        public List<Staff> Staff { get; set; }

        [JsonProperty("topic")]
        public List<Topic> Topics { get; set; }

        [JsonProperty("blog")]
        public List<Blog> Blogs { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Subject s = (Subject)obj;
            return Id == s.Id &&
                   Type == s.Type &&
                   Rank == s.Rank &&
                   EpsCount == s.EpsCount &&
                   AirWeekday == s.AirWeekday &&
                   Url.Equals(s.Url) &&
                   Name.Equals(s.Name) &&
                   NameCn.Equals(s.NameCn) &&
                   Summary.Equals(s.Summary) &&
                   AirDate.Equals(s.AirDate) &&
                   Rating.Equals(s.Rating) &&
                   Images.Equals(s.Images) &&
                   Collection.Equals(s.Collection) &&
                   Eps.SequenceEqual(s.Eps) &&
                   Characters.SequenceEqual(s.Characters) &&
                   Staff.SequenceEqual(s.Staff) &&
                   Topics.SequenceEqual(s.Topics) &&
                   Blogs.SequenceEqual(s.Blogs);

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
