using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class EpStatusEnumEx
    {
        public static string GetCnName(this EpStatusEnum status)
        {
            switch (status)
            {
                case EpStatusEnum.watched:
                    return "看过";
                case EpStatusEnum.queue:
                    return "想看";
                case EpStatusEnum.drop:
                    return "抛弃";
                case EpStatusEnum.remove:
                    return "";
                default:
                    return "";
            }
        }

        public static string GetCssName(this EpStatusEnum status)
        {
            switch (status)
            {
                case EpStatusEnum.watched:
                    return "Watched";
                case EpStatusEnum.queue:
                    return "Queue";
                case EpStatusEnum.drop:
                    return "Drop";
                case EpStatusEnum.remove:
                    return "";
                default:
                    return "";
            }
        }

        public static string GetUrlName(this EpStatusEnum status)
        {
            switch (status)
            {
                case EpStatusEnum.watched:
                    return "watched";
                case EpStatusEnum.queue:
                    return "queue";
                case EpStatusEnum.drop:
                    return "drop";
                case EpStatusEnum.remove:
                    return "";
                default:
                    return "";
            }
        }



    }
}
