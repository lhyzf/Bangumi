using Bangumi.Api.Models;
using Bangumi.Api.Services;
using Bangumi.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace Bangumi.Helper
{
    internal static class OAuthHelper
    {
        private const string OAuthBaseUrl = "https://bgm.tv/oauth";
        private const string RedirectUrl = Constants.RedirectUrl;
        private const string ClientId = Constants.ClientId;
        public static AccessToken MyToken = new AccessToken();
        public static bool IsLogin = false;

        /// <summary>
        /// 用户登录。
        /// </summary>
        /// <returns></returns>
        public static async Task Authorize()
        {
            try
            {
                string URL = $"{OAuthBaseUrl}/authorize?client_id=" + ClientId + "&response_type=code";

                Uri StartUri = new Uri(URL);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                Uri EndUri = new Uri($"{OAuthBaseUrl}/{RedirectUrl}");

                //rootPage.NotifyUser("Navigating to: " + GoogleURL, NotifyType.StatusMessage);

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    await GetAccessToken(WebAuthenticationResult.ResponseData.ToString().Replace($"{OAuthBaseUrl}/{RedirectUrl}?code=", ""));
                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    //OutputToken("HTTP Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseErrorDetail.ToString());
                }
                else
                {
                    //OutputToken("Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseStatus.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                //var msgDialog = new Windows.UI.Popups.MessageDialog("登录失败，请重试！\n" + e.Message) { Title = "登录失败！" };
                //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                //await msgDialog.ShowAsync();
                //rootPage.NotifyUser(Error.Message, NotifyType.ErrorMessage);
            }
        }

        /// <summary>
        /// 使用 code 换取 Access Token。
        /// </summary>
        /// <param name="codeString"></param>
        /// <returns></returns>
        private static async Task GetAccessToken(string code)
        {
            try
            {
                AccessToken token;
                // 重试最多三次
                for (int i = 0; i < 3; i++)
                {
                    Debug.WriteLine($"第{i + 1}次尝试获取Token。");
                    token = await BangumiHttpWrapper.GetAccessToken(code);
                    if (token != null)
                    {
                        await WriteTokensAsync(token);
                        break;
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("获取AccessToken失败。");
                throw e;
            }
        }

        /// <summary>
        /// 查询授权信息，并在满足条件时刷新Token。
        /// </summary>
        private static async void CheckAccessToken()
        {
            try
            {
                AccessToken token;
                // 重试最多三次
                for (int i = 0; i < 3; i++)
                {
                    Debug.WriteLine($"第{i + 1}次尝试刷新Token。");
                    token = await BangumiHttpWrapper.CheckAccessToken(MyToken);
                    if (token != null)
                    {
                        // 将信息写入本地文件
                        await WriteTokensAsync(token);
                        break;
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                Debug.WriteLine("response.StatusCode:" + response?.StatusCode);
                if (response?.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // 授权过期，返回登录界面
                    MainPage.rootFrame.Navigate(typeof(LoginPage), "ms-appx:///Assets/resource/err_401.png");
                    //var msgDialog = new Windows.UI.Popups.MessageDialog("登录已过期，请重新登录！") { Title = "登录失效！" };
                    //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                    //await msgDialog.ShowAsync();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 检查用户授权文件。
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CheckTokens()
        {
            MyToken.Token = await FileHelper.ReadFromFileAsync(getOAuthFileName(OAuthFile.access_token), true);
            MyToken.RefreshToken = await FileHelper.ReadFromFileAsync(getOAuthFileName(OAuthFile.refresh_token), true);
            MyToken.UserId = await FileHelper.ReadFromFileAsync(getOAuthFileName(OAuthFile.user_id), false);
            if (string.IsNullOrEmpty(MyToken.Token) || string.IsNullOrEmpty(MyToken.RefreshToken) || string.IsNullOrEmpty(MyToken.UserId))
            {
                //DeleteTokens();
                IsLogin = false;
                return false;
            }
            IsLogin = true;
            // 检查是否在有效期内，接近过期或过期则刷新token
            CheckAccessToken();
            return true;
        }

        /// <summary>
        /// 将 Token 写入内存及文件。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task WriteTokensAsync(AccessToken token)
        {
            if (!string.IsNullOrEmpty(token.Token) && !string.IsNullOrEmpty(token.RefreshToken) && !string.IsNullOrEmpty(token.UserId.ToString()))
            {
                // 刷新后存入内存
                MyToken.Token = token.Token;
                MyToken.RefreshToken = token.RefreshToken;
                MyToken.UserId = token.UserId;
                // 将信息写入本地文件
                await FileHelper.WriteToFileAsync(token.Token, getOAuthFileName(OAuthFile.access_token), true);
                await FileHelper.WriteToFileAsync(token.RefreshToken, getOAuthFileName(OAuthFile.refresh_token), true);
                await FileHelper.WriteToFileAsync(token.UserId.ToString(), getOAuthFileName(OAuthFile.user_id), false);
            }
        }

        /// <summary>
        /// 删除用户相关文件。
        /// </summary>
        /// <returns></returns>
        public static void DeleteTokens()
        {
            // 删除用户认证文件
            FileHelper.DeleteLocalFile(getOAuthFileName(OAuthFile.access_token));
            FileHelper.DeleteLocalFile(getOAuthFileName(OAuthFile.refresh_token));
            FileHelper.DeleteLocalFile(getOAuthFileName(OAuthFile.user_id));
            // 删除用户缓存文件
            FileHelper.DeleteCacheFile("JsonCache\\home");
            FileHelper.DeleteCacheFile("JsonCache\\anime");
            FileHelper.DeleteCacheFile("JsonCache\\book");
            FileHelper.DeleteCacheFile("JsonCache\\game");
            FileHelper.DeleteCacheFile("JsonCache\\music");
            FileHelper.DeleteCacheFile("JsonCache\\real");
        }

        #region FileName
        public static string getOAuthFileName(OAuthFile fileName)
        {
            string result = string.Empty;
            switch (fileName)
            {
                case OAuthFile.access_token:
                    result = "AccessToken.data";
                    break;
                case OAuthFile.refresh_token:
                    result = "RefreshToken.data";
                    break;
                case OAuthFile.user_id:
                    result = "UserId.data";
                    break;
            }
            return result;
        }

        public enum OAuthFile
        {
            access_token,
            refresh_token,
            user_id,
        };
        #endregion
    }
}
