using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class BangumiConverters
    {
        #region Value to Enum
        public static CollectionStatusEnum ConvertCollectionStatusToEnum(string status)
        {
            switch (status)
            {
                case "想看":
                    return CollectionStatusEnum.Wish;
                case "看过":
                    return CollectionStatusEnum.Collect;
                case "在看":
                    return CollectionStatusEnum.Do;
                case "搁置":
                    return CollectionStatusEnum.OnHold;
                case "抛弃":
                    return CollectionStatusEnum.Dropped;
                default:
                    return CollectionStatusEnum.No;
            }
        }
        #endregion

    }
}
