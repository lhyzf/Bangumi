using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Models
{
    public static class EpStatusEnumEx
    {
        public static string GetValue(this EpStatusEnum status)
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
    }
}
