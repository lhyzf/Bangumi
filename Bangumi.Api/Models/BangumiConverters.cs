using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public static class BangumiConverters
    {
        #region Value to Enum
        public static CollectionStatusEnum GetCollectionStatusEnum(string status)
        {
            switch (status)
            {
                case "想看":
                    return CollectionStatusEnum.wish;
                case "看过":
                    return CollectionStatusEnum.collect;
                case "在看":
                    return CollectionStatusEnum.@do;
                case "搁置":
                    return CollectionStatusEnum.on_hold;
                case "抛弃":
                    return CollectionStatusEnum.dropped;
                default:
                    return CollectionStatusEnum.no;
            }
        }
        #endregion

    }
}
