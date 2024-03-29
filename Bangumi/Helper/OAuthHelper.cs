﻿using Bangumi.Api;
using Bangumi.Api.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Bangumi.Helper
{
    internal static class OAuthHelper
    {
        /// <summary>
        /// 用户登录。
        /// </summary>
        /// <returns></returns>
        [Obsolete("网站登录页面脚本在 IE11 下无法正常运行")]
        public static async Task Authorize()
        {
            try
            {
                string url = $"{BgmOAuth.OAuthHOST}/authorize?client_id={BangumiApi.BgmOAuth.ClientId}&response_type=code";

                Uri startUri = new Uri(url);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                Uri endUri = new Uri($"{BgmOAuth.OAuthHOST}/{BangumiApi.BgmOAuth.RedirectUrl}");

                //rootPage.NotifyUser("Navigating to: " + GoogleURL, NotifyType.StatusMessage);

                WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
                if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    await BangumiApi.BgmOAuth.GetToken(webAuthenticationResult.ResponseData.Replace($"{BgmOAuth.OAuthHOST}/{BangumiApi.BgmOAuth.RedirectUrl}?code=", ""));
                }
                else if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    throw new Exception("HTTP Error: " + webAuthenticationResult.ResponseErrorDetail.ToString());
                }
                else
                {
                    throw new Exception("Error: " + webAuthenticationResult.ResponseStatus.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("换取Token失败。");
                Debug.WriteLine(e.StackTrace);
                throw;
            }
        }


    }
}
