using Bangumi.Api.Models;
using Bangumi.Api.Services;
using Bangumi.Views;
using Newtonsoft.Json;
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
        public static AccessToken MyToken;
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
                        if (!token.Equals(MyToken))
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
            if (MyToken == null)
                MyToken = JsonConvert.DeserializeObject<AccessToken>(await FileHelper.ReadFromFileAsync(OAuthFile.token.GetFilePath(), true));
            if (MyToken == null)
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
            // 存入内存
            MyToken = token;
            IsLogin = true;
            // 将信息写入本地文件
            await FileHelper.WriteToFileAsync(JsonConvert.SerializeObject(token), OAuthFile.token.GetFilePath(), true);
        }

        /// <summary>
        /// 删除用户相关文件。
        /// </summary>
        /// <returns></returns>
        public static void DeleteTokens()
        {
            // 删除用户认证文件
            FileHelper.DeleteLocalFile(OAuthFile.token.GetFilePath());
            // 删除用户缓存文件
            FileHelper.DeleteCacheFile(CacheFile.progress.GetFilePath());
            FileHelper.DeleteCacheFile(CacheFile.anime.GetFilePath());
            FileHelper.DeleteCacheFile(CacheFile.book.GetFilePath());
            FileHelper.DeleteCacheFile(CacheFile.game.GetFilePath());
            FileHelper.DeleteCacheFile(CacheFile.music.GetFilePath());
            FileHelper.DeleteCacheFile(CacheFile.real.GetFilePath());
        }

        #region JsonCacheFile
        public enum CacheFile
        {
            progress,
            anime,
            book,
            game,
            music,
            real,
            calendar,
        }

        public static string GetFilePath(this CacheFile file)
        {
            switch (file)
            {
                case CacheFile.progress:
                    return "JsonCache\\progress";
                case CacheFile.anime:
                    return "JsonCache\\anime";
                case CacheFile.book:
                    return "JsonCache\\book";
                case CacheFile.game:
                    return "JsonCache\\game";
                case CacheFile.music:
                    return "JsonCache\\music";
                case CacheFile.real:
                    return "JsonCache\\real";
                case CacheFile.calendar:
                    return "JsonCache\\calendar";
                default:
                    return string.Empty;
            }
        }

        public static string GetFilePath(this SubjectTypeEnum subjectType)
        {
            switch (subjectType)
            {
                case SubjectTypeEnum.book:
                    return "JsonCache\\book";
                case SubjectTypeEnum.anime:
                    return "JsonCache\\anime";
                case SubjectTypeEnum.music:
                    return "JsonCache\\music";
                case SubjectTypeEnum.game:
                    return "JsonCache\\game";
                case SubjectTypeEnum.real:
                    return "JsonCache\\real";
                default:
                    return string.Empty;
            }
        }
        #endregion

        #region OAuthFile
        public enum OAuthFile
        {
            token,
        };

        public static string GetFilePath(this OAuthFile file)
        {
            switch (file)
            {
                case OAuthFile.token:
                    return "Token.data";
                default:
                    return string.Empty;
            }
        }
        #endregion
    }
}
