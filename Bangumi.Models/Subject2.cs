using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Models
{
    public class Subject2
    {
        [JsonProperty("subject_id")]
        public int SubjectId { get; set; }

        [JsonProperty("subject")]
        public Subject Subject { get; set; }
    }
}
