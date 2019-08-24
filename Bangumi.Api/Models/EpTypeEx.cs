using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class EpTypeEx
    {        
        /// <summary>
        /// 获取章节类型的描述
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetEpisodeType(this int type)
        {
            switch (type)
            {
                case 0:
                    return "本篇";
                case 1:
                    return "特别篇";
                case 2:
                    return "OP";
                case 3:
                    return "ED";
                case 4:
                    return "预告/宣传/广告";
                case 5:
                    return "MAD";
                case 6:
                    return "其他";
                default:
                    return "";
            }
        }
    }
}
