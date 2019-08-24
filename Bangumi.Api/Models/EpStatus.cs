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
                   (CssName == null ? CssName == e.CssName : CssName.Equals(e.CssName)) &&
                   (UrlName == null ? UrlName == e.UrlName : UrlName.Equals(e.UrlName)) &&
                   (CnName == null ? CnName == e.CnName : CnName.Equals(e.CnName));
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
