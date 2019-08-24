﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class Topic
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("main_id")]
        public int MainId { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("lastpost")]
        public long LastPost { get; set; }

        [JsonProperty("replies")]
        public int Replies { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Topic t = (Topic)obj;
            return Id == t.Id &&
                   MainId == t.MainId &&
                   Timestamp == t.Timestamp &&
                   LastPost == t.LastPost &&
                   Replies == t.Replies &&
                   (Url == null ? Url == t.Url : Url.Equals(t.Url)) &&
                   (Title == null ? Title == t.Title : Title.Equals(t.Title)) &&
                   (User == null ? User == t.User : User.Equals(t.User));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
