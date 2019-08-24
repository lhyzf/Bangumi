using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 章节状态
    /// </summary>
    public class EpStatus
    {
        public EpStatus()
        {
            CssName = string.Empty;
            UrlName = string.Empty;
            CnName = string.Empty;
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("css_name")]
        public string CssName { get; set; }

        [JsonProperty("url_name")]
        public string UrlName { get; set; }

        [JsonProperty("cn_name")]
        public string CnName { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EpStatus e = (EpStatus)obj;
            return Id == e.Id &&
                   CssName.Equals(e.CssName) &&
                   UrlName.Equals(e.UrlName) &&
                   CnName.Equals(e.CnName);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
