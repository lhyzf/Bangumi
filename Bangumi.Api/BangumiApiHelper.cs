using Bangumi.Api.Models;
using Bangumi.Api.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Bangumi.Api
{
    /// <summary>
    /// 提供访问 Api 以及缓存管理
    /// </summary>
    public static class BangumiApiHelper
    {
        private static BangumiHttpWrapper wrapper;
        // 本地文件夹路径，永久保存
        private static string localFolderPath;
        // 缓存文件夹路径
        private static string cacheFolderPath;

        public static string OAuthBaseUrl => BangumiHttpWrapper.OAuthBaseUrl;
        public static string ClientId => BangumiHttpWrapper.ClientId;
        public static string RedirectUrl => BangumiHttpWrapper.RedirectUrl;
        public static AccessToken MyToken { get; private set; }
        public static bool IsLogin
        {
            get => MyToken != null;
        }

        /// <summary>
        /// 初始化帮助类
        /// </summary>
        /// <param name="localFolder"></param>
        /// <param name="cacheFolder"></param>
        /// <param name="baseUrl"></param>
        /// <param name="oAuthBaseUrl"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="redirectUrl"></param>
        /// <param name="noImageUri"></param>
        public static void Init(string localFolder,
                                string cacheFolder,
                                string baseUrl,
                                string oAuthBaseUrl,
                                string clientId,
                                string clientSecret,
                                string redirectUrl,
                                string noImageUri)
        {
            if (wrapper == null)
            {
                localFolderPath = localFolder;
                cacheFolderPath = cacheFolder;
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
            //await FileHelper.EncryptAndWriteFileAsync(localFolderPath + "\\test.data",
            //                            redirectUrl);
            //await FileHelper.ReadAndDecryptFileAsync(localFolderPath + "\\test.data");
        }


        #region OAuth 相关方法

        #region public
        /// <summary>
        /// 使用 code 换取 Access Token。
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task GetTokenAsync(string code)
        {
            AccessToken token;
            // 重试最多三次
            for (int i = 0; i < 3; i++)
            {
                Debug.WriteLine($"第{i + 1}次尝试获取Token。");
                token = await BangumiHttpWrapper.GetTokenAsync(code);
                if (token != null)
                {
                    await WriteTokenAsync(token);
                    break;
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        /// 检查用户授权文件。
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CheckMyToken()
        {
            if (MyToken == null)
                MyToken = JsonConvert.DeserializeObject<AccessToken>(await FileHelper.ReadAndDecryptFileAsync(localFolderPath + "\\token.data"));
            if (MyToken == null)
            {
                //DeleteTokens();
                return false;
            }
            // 检查是否在有效期内，接近过期或过期则刷新token
            _ = CheckToken();
            return true;
        }

        /// <summary>
        /// 删除用户相关文件。
        /// </summary>
        /// <returns></returns>
        public static void DeleteToken()
        {
            // 删除用户认证文件
            MyToken = null;
            FileHelper.DeleteFile(localFolderPath + "\\token.data");
            // 删除用户缓存文件
            //FileHelper.DeleteCacheFile(CacheFile.Progress.GetFilePath());
            //FileHelper.DeleteCacheFile(CacheFile.Anime.GetFilePath());
            //FileHelper.DeleteCacheFile(CacheFile.Book.GetFilePath());
            //FileHelper.DeleteCacheFile(CacheFile.Game.GetFilePath());
            //FileHelper.DeleteCacheFile(CacheFile.Music.GetFilePath());
            //FileHelper.DeleteCacheFile(CacheFile.Real.GetFilePath());
        }
        #endregion

        #region private
        /// <summary>
        /// 查询授权信息，并在满足条件时刷新Token。
        /// </summary>
        private static async Task CheckToken()
        {
            try
            {
                AccessToken token;
                Debug.WriteLine("尝试刷新Token。");
                token = await BangumiHttpWrapper.CheckTokenAsync(MyToken);
                if (token != null)
                {
                    // 将信息写入本地文件
                    if (!token.Equals(MyToken))
                        await WriteTokenAsync(token);
                }
            }
            catch (Exception e)
            {
                MyToken = null;
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 将 Token 写入内存及文件。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task WriteTokenAsync(AccessToken token)
        {
            // 存入内存
            MyToken = token;
            // 将信息写入本地文件
            await FileHelper.EncryptAndWriteFileAsync(localFolderPath + "\\token.data",
                                                      JsonConvert.SerializeObject(token));
        }
        #endregion

        #endregion



    }
}
