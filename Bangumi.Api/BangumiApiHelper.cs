using Bangumi.Api.Models;
using Bangumi.Api.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Bangumi.Api
{
    /// <summary>
    /// 提供 Api 访问以及缓存管理
    /// </summary>
    public static class BangumiApiHelper
    {
        /// <summary>
        /// Http 请求封装
        /// </summary>
        private static BangumiHttpWrapper wrapper;

        /// <summary>
        /// 本地文件夹路径，永久保存
        /// </summary>
        private static string localFolderPath;

        /// <summary>
        /// 缓存文件夹路径
        /// </summary>
        private static string cacheFolderPath;

        /// <summary>
        /// 用来表示 Token 过期或不可用
        /// </summary>
        private static bool isLogin = false;

        /// <summary>
        /// 记录缓存是否更新过
        /// </summary>
        private static bool isCacheUpdated = false;

        /// <summary>
        /// 定时器
        /// </summary>
        private static Timer timer;

        public static string OAuthBaseUrl => BangumiHttpWrapper.OAuthBaseUrl;
        public static string ClientId => BangumiHttpWrapper.ClientId;
        public static string RedirectUrl => BangumiHttpWrapper.RedirectUrl;

        /// <summary>
        /// 用户认证存在且可用
        /// </summary>
        public static bool IsLogin
        {
            get => MyToken != null && isLogin;
        }

        /// <summary>
        /// 存储 Token
        /// </summary>
        public static AccessToken MyToken { get; private set; }

        /// <summary>
        /// 存储缓存数据
        /// </summary>
        public static BangumiCache BangumiCache { get; private set; }

        /// <summary>
        /// 初始化帮助类
        /// </summary>
        /// <param name="localFolder">本地文件夹</param>
        /// <param name="cacheFolder">缓存文件夹</param>
        /// <param name="baseUrl"></param>
        /// <param name="oAuthBaseUrl"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="redirectUrl"></param>
        /// <param name="noImageUri">无图片时显示的图片路径</param>
        public static async void Init(string localFolder,
                                      string cacheFolder,
                                      string baseUrl,
                                      string oAuthBaseUrl,
                                      string clientId,
                                      string clientSecret,
                                      string redirectUrl,
                                      string noImageUri)
        {
            // 实例化 Http 封装
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
            // 加载缓存
            if (BangumiCache == null)
            {
                BangumiCache = new BangumiCache();
                if (File.Exists(JsonCacheFile.BangumiCache.GetFilePath(cacheFolderPath)))
                {
                    try
                    {
                        BangumiCache = JsonConvert.DeserializeObject<BangumiCache>(await FileHelper.ReadTextAsync(JsonCacheFile.BangumiCache.GetFilePath(cacheFolderPath)));
                    }
                    catch (Exception)
                    {
                        FileHelper.DeleteFile(JsonCacheFile.BangumiCache.GetFilePath(cacheFolderPath));
                    }
                }
            }
            // 启动定时器，定时将缓存写入文件
            timer = new Timer(5000);
            timer.Elapsed += WriteCacheToFileTimer_Elapsed;
            timer.AutoReset = true;
            timer.Start();

            //await FileHelper.EncryptAndWriteFileAsync(localFolderPath + "\\test.data",
            //                            redirectUrl);
            //await FileHelper.ReadAndDecryptFileAsync(localFolderPath + "\\test.data");
        }

        private static async void WriteCacheToFileTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isCacheUpdated)
            {
                isCacheUpdated = false;
                await FileHelper.WriteTextAsync(JsonCacheFile.BangumiCache.GetFilePath(cacheFolderPath),
                                                JsonConvert.SerializeObject(BangumiCache));
            }
        }

        #region Api 请求
        /// <summary>
        /// 获取指定类别收藏信息。
        /// </summary>
        /// <param name="userIdString"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public static async Task<Collection2> GetSubjectCollectionAsync(SubjectTypeEnum subjectType)
        {
            try
            {
                var result = await BangumiHttpWrapper.GetSubjectCollectionAsync(MyToken.UserId, subjectType);
                UpdateCache(BangumiCache.Collections, subjectType.GetValue(), result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectCollectionAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="accessTokenString"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<SubjectStatus2> GetCollectionStatusAsync(string subjectId)
        {
            try
            {
                var result = await BangumiHttpWrapper.GetCollectionStatusAsync(MyToken.Token, subjectId);
                UpdateCache(BangumiCache.SubjectStatus, subjectId, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetCollectionStatusAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="userIdString"></param>
        /// <param name="accessTokenString"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Progress> GetProgressesAsync(string subjectId)
        {
            try
            {
                var result = await BangumiHttpWrapper.GetProgressesAsync(MyToken.UserId, MyToken.Token, subjectId);
                UpdateCache(BangumiCache.Progresses, subjectId, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetProgressesAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取用户收视列表。
        /// </summary>
        /// <param name="userIdString"></param>
        /// <returns></returns>
        public static async Task<List<Watching>> GetWatchingListAsync()
        {
            try
            {
                var result = await BangumiHttpWrapper.GetWatchingListAsync(MyToken.UserId);
                UpdateCache(BangumiCache.Watchings, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetWatchingListAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 更新指定条目收藏状态。
        /// </summary>
        /// <param name="accessTokenString"></param>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatusEnum"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privace"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateCollectionStatusAsync(string subjectId,
                                                                   CollectionStatusEnum collectionStatusEnum,
                                                                   string comment = "",
                                                                   string rating = "",
                                                                   string privace = "0")
        {
            try
            {
                var result = await BangumiHttpWrapper.UpdateCollectionStatusAsync(MyToken.Token, subjectId,
                                    collectionStatusEnum, comment, rating, privace);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateCollectionStatusAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 更新收视进度。
        /// </summary>
        /// <param name="accessTokenString"></param>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressAsync(string ep, EpStatusEnum status)
        {
            try
            {
                var result = await BangumiHttpWrapper.UpdateProgressAsync(MyToken.Token, ep, status);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 批量更新收视进度。
        /// 使用 HttpWebRequest 提交表单进行更新，更新收藏状态使用相同方法。
        /// </summary>
        /// <param name="accessTokenString"></param>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <param name="epsId"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressBatchAsync(int ep, EpStatusEnum status, string epsId)
        {
            try
            {
                var result = await BangumiHttpWrapper.UpdateProgressBatchAsync(MyToken.Token, ep, status, epsId);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressBatchAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目所有章节。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubjectEpsAsync(string subjectId)
        {
            try
            {
                var result = await BangumiHttpWrapper.GetSubjectEpsAsync(subjectId);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectEpsAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目详情。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubjectAsync(string subjectId)
        {
            try
            {
                var result = await BangumiHttpWrapper.GetSubjectAsync(subjectId);
                UpdateCache(BangumiCache.Subjects, subjectId, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectAsync Error.");
                throw e;
            }
        }

        /// <summary>
        /// 获取时间表。
        /// </summary>
        /// <returns></returns>
        public static async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            try
            {
                var result = await BangumiHttpWrapper.GetBangumiCalendarAsync();
                UpdateCache(BangumiCache.TimeLine, result);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetBangumiCalendarAsync Error.");
                throw e;
            }
        }
        /// <summary>
        /// 获取搜索结果。
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static async Task<SearchResult> GetSearchResultAsync(string keyWord, string type, int start, int n)
        {
            try
            {
                var result = await BangumiHttpWrapper.GetSearchResultAsync(keyWord, type, start, n);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSearchResultAsync Error.");
                throw e;
            }
        }

        #region 更新缓存方法
        /// <summary>
        /// 更新缓存，字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void UpdateCache<T>(Dictionary<string, T> dic, string key, T value)
        {
            if (value != null)
            {
                if (dic.ContainsKey(key))
                {
                    if (!value.Equals(dic[key]))
                    {
                        dic[key] = value;
                        timer.Interval = 5000;
                        isCacheUpdated = true;
                    }
                }
                else
                {
                    dic.Add(key, value);
                    timer.Interval = 5000;
                    isCacheUpdated = true;
                }
            }
        }

        /// <summary>
        /// 更新缓存，列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private static void UpdateCache<T>(List<T> source, List<T> dest)
        {
            if (!source.SequenceEqual(dest))
            {
                source.Clear();
                source.AddRange(dest);
                // 重新计时 5 秒
                timer.Interval = 5000;
                isCacheUpdated = true;
            }
        }

        #endregion

        #endregion


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
            {
                MyToken = JsonConvert.DeserializeObject<AccessToken>(await FileHelper.ReadAndDecryptFileAsync(localFolderPath + "\\token.data"));
                if (MyToken == null)
                {
                    //DeleteTokens();
                    return false;
                }
                isLogin = true;
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
                if (!e.Message.Contains("无法解析服务器的名称或地址"))
                {
                    isLogin = false;
                }
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
            isLogin = true;
            // 将信息写入本地文件
            await FileHelper.EncryptAndWriteFileAsync(localFolderPath + "\\token.data",
                                                      JsonConvert.SerializeObject(token));
        }
        #endregion

        #endregion

        #region JsonCacheFile
        public enum JsonCacheFile
        {
            BangumiCache,
        }

        private static string GetFilePath(this JsonCacheFile file, string folder)
        {
            return $"{folder}\\{file.ToString().ToLower()}.json";
        }

        #endregion


    }
}
