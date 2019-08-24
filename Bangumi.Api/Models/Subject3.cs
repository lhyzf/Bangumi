﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 简略条目详情，适用于Watching
    /// </summary>
    public class Subject3
    {
        public Subject3()
        {
            Url = string.Empty;
            Name = string.Empty;
            NameCn = string.Empty;
            AirDate = string.Empty;
            Images = new Images();
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

        [JsonProperty("eps_count")]
        public int EpsCount { get; set; }

        [JsonProperty("air_date")]
        public string AirDate { get; set; }

        [JsonProperty("air_weekday")]
        public int AirWeekday { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Subject3 s = (Subject3)obj;
            return Id == s.Id &&
                   Type == s.Type &&
                   EpsCount == s.EpsCount &&
                   AirWeekday == s.AirWeekday &&
                   Url.Equals(s.Url) &&
                   Name.Equals(s.Name) &&
                   NameCn.Equals(NameCn) &&
                   AirDate.Equals(s.AirDate) &&
                   Images.Equals(s.Images);

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
