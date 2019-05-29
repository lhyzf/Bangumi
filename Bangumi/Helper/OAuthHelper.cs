using Bangumi.Models;
using Bangumi.Services;
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
        private const string client_id = Constants.ClientId;
        private const string client_secret = Constants.ClientSecret;
        private const string redirect_url = Constants.RedirectUrl;
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
                    await WriteTokensAsync(result);
                }
                else //再试一次
                {
                    response = await HttpHelper.PostAsync(url, postData);
                    if (!string.IsNullOrEmpty(response))
                    {
                        var result = JsonConvert.DeserializeObject<AccessToken>(response);
                        //将信息写入本地文件
                        await WriteTokensAsync(result);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
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
                    AccessTokenString = result.Token;
                    RefreshTokenString = result.RefreshToken;
                    UserIdString = result.UserId.ToString();
                    // 将信息写入本地文件
                    await WriteTokensAsync(result);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
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
                    if (result.Expires < aa)
                        await RefreshAccessToken();
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                Debug.WriteLine("response.StatusCode:" + response?.StatusCode);
                if (response?.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var msgDialog = new Windows.UI.Popups.MessageDialog("登录已过期，请重新登录！") { Title = "登录失效！" };
                    msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                    await msgDialog.ShowAsync();
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
            AccessTokenString = await FileHelper.ReadFromFileAsync(getOAuthFileName(OAuthFile.access_token), true);
            RefreshTokenString = await FileHelper.ReadFromFileAsync(getOAuthFileName(OAuthFile.refresh_token), true);
            UserIdString = await FileHelper.ReadFromFileAsync(getOAuthFileName(OAuthFile.user_id), false);
            if (string.IsNullOrEmpty(AccessTokenString) || string.IsNullOrEmpty(RefreshTokenString) || string.IsNullOrEmpty(UserIdString))
            {
                DeleteTokens();
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
        private static async Task WriteTokensAsync(AccessToken token)
        {
            if (!string.IsNullOrEmpty(token.Token) && !string.IsNullOrEmpty(token.RefreshToken) && !string.IsNullOrEmpty(token.UserId.ToString()))
            {
                await FileHelper.WriteToFileAsync(token.Token, getOAuthFileName(OAuthFile.access_token), true);
                await FileHelper.WriteToFileAsync(token.RefreshToken, getOAuthFileName(OAuthFile.refresh_token), true);
                await FileHelper.WriteToFileAsync(token.UserId.ToString(), getOAuthFileName(OAuthFile.user_id), false);
            }
        }

        /// <summary>
        /// 删除 Tokens。
        /// </summary>
        /// <returns></returns>
        public static void DeleteTokens()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (File.Exists(localFolder.Path + "\\" + getOAuthFileName(OAuthFile.access_token)))
                File.Delete(localFolder.Path + "\\" + getOAuthFileName(OAuthFile.access_token));
            if (File.Exists(localFolder.Path + "\\" + getOAuthFileName(OAuthFile.refresh_token)))
                File.Delete(localFolder.Path + "\\" + getOAuthFileName(OAuthFile.refresh_token));
            if (File.Exists(localFolder.Path + "\\" + getOAuthFileName(OAuthFile.user_id)))
                File.Delete(localFolder.Path + "\\" + getOAuthFileName(OAuthFile.user_id));
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
