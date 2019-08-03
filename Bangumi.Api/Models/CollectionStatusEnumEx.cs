using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class CollectionStatusEnumEx
    {
        public static string GetDescCn(this CollectionStatusEnum status)
        {
            switch (status)
            {
                case CollectionStatusEnum.Wish:
                    return "想看";
                case CollectionStatusEnum.Collect:
                    return "看过";
                case CollectionStatusEnum.Do:
                    return "在看";
                case CollectionStatusEnum.OnHold:
                    return "搁置";
                case CollectionStatusEnum.Dropped:
                    return "抛弃";
                default:
                    return "收藏";
            }
        }

        public static string GetValue(this CollectionStatusEnum status)
        {
            switch (status)
            {
                case CollectionStatusEnum.Wish:
                    return "wish";
                case CollectionStatusEnum.Collect:
                    return "collect";
                case CollectionStatusEnum.Do:
                    return "do";
                case CollectionStatusEnum.OnHold:
                    return "on_hold";
                case CollectionStatusEnum.Dropped:
                    return "dropped";
                default:
                    return "";
            }
        }
    }
}
