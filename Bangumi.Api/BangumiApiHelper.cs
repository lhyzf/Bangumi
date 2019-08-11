using Bangumi.Api.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api
{
    /// <summary>
    /// 提供访问 Api 以及缓存管理
    /// </summary>
    public static class BangumiApiHelper
    {
        private static BangumiHttpWrapper wrapper;
        private static string folderPath;

        public static string OAuthBaseUrl => BangumiHttpWrapper.OAuthBaseUrl;
        public static string ClientId => BangumiHttpWrapper.ClientId;
        public static string RedirectUrl => BangumiHttpWrapper.RedirectUrl;

        /// <summary>
        /// 初始化帮助类
        /// </summary>
        public static void Init(string folder,
                                string baseUrl,
                                string oAuthBaseUrl,
                                string clientId,
                                string clientSecret,
                                string redirectUrl,
                                string noImageUri)
        {
            if (wrapper == null)
            {
                folderPath = folder;
                wrapper = new BangumiHttpWrapper
                {
                    //BaseUrl = baseUrl,
                    //OAuthBaseUrl = oAuthBaseUrl,
                    //ClientId = clientId,
                    //ClientSecret = clientSecret,
                    //RedirectUrl = redirectUrl,
                    //NoImageUri = noImageUrl
                };

                // 临时
                BangumiHttpWrapper.BaseUrl = baseUrl;
                BangumiHttpWrapper.OAuthBaseUrl = oAuthBaseUrl;
                BangumiHttpWrapper.ClientId = clientId;
                BangumiHttpWrapper.ClientSecret = clientSecret;
                BangumiHttpWrapper.RedirectUrl = redirectUrl;
                BangumiHttpWrapper.NoImageUri = noImageUri;
            }
        }



    }
}
