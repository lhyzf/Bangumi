using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Services
{
    public static class Utils
    {
        /// <summary>
        /// 将C#时间戳转换为js时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ConvertDateTimeToJsTick(DateTime dateTime)
        {
            // C# 时间戳为 1/10000000 秒，从0001年1月1日开始；Bangumi 使用的时间戳为秒，从1970年1月1日开始
            // 默认 DateTime 使用 UTC 时间
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
