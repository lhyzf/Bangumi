using Bangumi.Api.Common;
using System.Text.Json.Serialization;

namespace Bangumi.Api.Models
{
    /// <summary>
    /// 章节状态
    /// <br/>2 = watched = 看过
    /// <br/>1 = queue = 想看
    /// <br/>3 = drop = 抛弃
    /// <br/>? = remove = 撤销
    /// </summary>
    public class EpStatus
    {
        /// <summary>
        /// 章节状态 ID
        /// </summary>
        [JsonPropertyName("id")]
        public EpStatusType Id { get; set; }

        /// <summary>
        /// [ Watched, Queue, Drop ]
        /// </summary>
        [JsonPropertyName("css_name")]
        public string CssName { get; set; }

        /// <summary>
        /// 章节状态类型
        /// <br/>[ watched, queue, drop, remove ]
        /// </summary>
        [JsonPropertyName("url_name")]
        public string UrlName { get; set; }

        /// <summary>
        /// 章节状态名称
        /// <br/>[ 看过, 想看, 抛弃, 撤销 ]
        /// </summary>
        [JsonPropertyName("cn_name")]
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
                   CssName.EqualsExT(e.CssName) &&
                   UrlName.EqualsExT(e.UrlName) &&
                   CnName.EqualsExT(e.CnName);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (int)Id;
        }
    }
}
