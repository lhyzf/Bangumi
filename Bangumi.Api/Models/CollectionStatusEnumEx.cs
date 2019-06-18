using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class CollectionStatusEnumEx
    {
        public static string GetValue(this CollectionStatusEnum status)
        {
            switch (status)
            {
                case CollectionStatusEnum.wish:
                    return "想看";
                case CollectionStatusEnum.collect:
                    return "看过";
                case CollectionStatusEnum.@do:
                    return "在看";
                case CollectionStatusEnum.on_hold:
                    return "搁置";
                case CollectionStatusEnum.dropped:
                    return "抛弃";
                default:
                    return "收藏";
            }
        }
    }
}
