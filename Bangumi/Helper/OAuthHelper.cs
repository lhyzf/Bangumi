using Bangumi.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace Bangumi.Helper
{
    internal static class OAuthHelper
    {
        private const string oauthBaseUrl = "https://bgm.tv/oauth";
        private const string client_id = Constants.client_id;
        private const string client_secret = Constants.client_secret;
        private const string redirect_url = Constants.redirect_url;
        public static string AccessTokenString = "";
        public static string RefreshTokenString = "";
        public static string UserIdString = "";
        public static bool IsLogin = false;

        /// <summary>
        /// 用户登录。
        /// </summary>
        /// <returns></returns>
        public static async Task Authorize()
        {
            try
            {
                string URL = $"{oauthBaseUrl}/authorize?client_id=" + client_id + "&response_type=code";

                Uri StartUri = new Uri(URL);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                Uri EndUri = new Uri($"{oauthBaseUrl}/{redirect_url}");

                //rootPage.NotifyUser("Navigating to: " + GoogleURL, NotifyType.StatusMessage);

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    await GetAccessToken(WebAuthenticationResult.ResponseData.ToString().Replace($"{oauthBaseUrl}/{redirect_url}?code=", ""));
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
            catch (Exception)
            {
                //rootPage.NotifyUser(Error.Message, NotifyType.ErrorMessage);
            }
        }

        /// <summary>
        /// 使用返回的的 code 换取 Access Token。
        /// </summary>
        /// <param name="codeString"></param>
        /// <returns></returns>
        private static async Task GetAccessToken(string codeString)
        {
            string url = $"{oauthBaseUrl}/access_token";
            string postData = "grant_type=authorization_code";
            postData += "&client_id=" + client_id;
            postData += "&client_secret=" + client_secret;
            postData += "&code=" + codeString;
            postData += "&redirect_uri=" + redirect_url;

            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                if (!string.IsNullOrEmpty(response))
                {
                    var result = JsonConvert.DeserializeObject<AccessToken>(response);
                    //将信息写入本地文件
                    await WriteTokens(result);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }


        /// <summary>
        /// 刷新授权有效期。
        /// </summary>
        /// <returns></returns>
        public static async Task RefreshAccessToken()
        {
            string url = $"{oauthBaseUrl}/access_token";
            string postData = "grant_type=refresh_token";
            postData += "&client_id=" + client_id;
            postData += "&client_secret=" + client_secret;
            postData += "&refresh_token=" + RefreshTokenString;
            postData += "&redirect_uri=" + redirect_url;

            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                if (!string.IsNullOrEmpty(response))
                {
                    var result = JsonConvert.DeserializeObject<AccessToken>(response);
                    // 刷新后存入内存
                    AccessTokenString = result.access_token;
                    RefreshTokenString = result.refresh_token;
                    UserIdString = result.user_id.ToString();
                    // 将信息写入本地文件
                    await WriteTokens(result);
                }
                else
                {
                    //授权信息已失效，要求重新登录
                    IsLogin = false;
                    await Authorize();
                    await CheckTokens();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// 查询授权信息。
        /// </summary>
        private static async void CheckAccessToken()
        {
            var token = AccessTokenString;
            string url = string.Format("{0}/token_status?access_token={1}", oauthBaseUrl, token);

            try
            {
                string response = await HttpHelper.PostAsync(url);
                if (string.IsNullOrEmpty(response))
                {
                    await RefreshAccessToken();
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<AccessToken>(response);
                    // C# 时间戳为 1/10000000 秒，从0001年1月1日开始；js 时间戳为秒，从1970年1月1日开始
                    // 获取两天后的时间戳，离过期不足两天时或过期后更新 access_token
                    var aa = (DateTime.Now.AddDays(2).ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;
                    if (result.expires < aa)
                        await RefreshAccessToken();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// 检查用户授权文件。
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CheckTokens()
        {
            AccessTokenString = await FileHelper.ReadFromFile(getOAuthFileName(OAuthFile.access_token), true);
            RefreshTokenString = await FileHelper.ReadFromFile(getOAuthFileName(OAuthFile.refresh_token), true);
            UserIdString = await FileHelper.ReadFromFile(getOAuthFileName(OAuthFile.user_id), false);
            if (string.IsNullOrEmpty(AccessTokenString) || string.IsNullOrEmpty(RefreshTokenString) || string.IsNullOrEmpty(UserIdString))
            {
                await DeleteTokens();
                IsLogin = false;
                return false;
            }
            IsLogin = true;
            // 检查是否在有效期内，接近过期或过期则刷新token
            CheckAccessToken();
            return true;
        }

        /// <summary>
        /// 写入 Tokens。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task WriteTokens(AccessToken token)
        {
            if (!string.IsNullOrEmpty(token.access_token) && !string.IsNullOrEmpty(token.refresh_token) && !string.IsNullOrEmpty(token.user_id.ToString()))
            {
                await FileHelper.WriteToFile(token.access_token, getOAuthFileName(OAuthFile.access_token), true);
                await FileHelper.WriteToFile(token.refresh_token, getOAuthFileName(OAuthFile.refresh_token), true);
                await FileHelper.WriteToFile(token.user_id.ToString(), getOAuthFileName(OAuthFile.user_id), false);
            }
        }

        /// <summary>
        /// 删除 Tokens。
        /// </summary>
        /// <returns></returns>
        public static async Task DeleteTokens()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile File = await localFolder.CreateFileAsync(getOAuthFileName(OAuthFile.access_token),
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
            File = await localFolder.CreateFileAsync(getOAuthFileName(OAuthFile.refresh_token),
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
            File = await localFolder.CreateFileAsync(getOAuthFileName(OAuthFile.user_id),
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
        }

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

    }
}
