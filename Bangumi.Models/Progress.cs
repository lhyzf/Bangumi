using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Models
{
    /// <summary>
    /// 某一条目用户收视进度
    /// </summary>
    public class Progress
    {
        [JsonProperty("subject_id")]
        public int SubjectId { get; set; }

        [JsonProperty("eps")]
        public List<EpStatus2> Eps { get; set; }
    }
}
