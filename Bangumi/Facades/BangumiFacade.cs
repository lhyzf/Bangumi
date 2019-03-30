using Bangumi.Helper;
using Bangumi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bangumi.Facades
{
    class BangumiFacade
    {
        private const string BaseUrl = "https://api.bgm.tv";
        private const string NoImageUri = "ms-appx:///Assets/NoImage.png";
        
        /// <summary>
        /// 显示用户选定类别收藏信息
        /// </summary>
        /// <param name="subjectCollection"></param>
        /// <param name="username"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public static async Task<bool> PopulateSubjectCollectionAsync(ObservableCollection<Collect> subjectCollection,
            string username, SubjectType subjectType)
        {
            try
            {
                var subjectCollections = await GetSubjectCollectionAsync(username, subjectType);
                //清空原数据
                subjectCollection.Clear();
                foreach (var subjects in subjectCollections.collects)
                {
                    foreach (var item in subjects.list)
                    {
                        if (string.IsNullOrEmpty(item.subject.name_cn))
                        {
                            item.subject.name_cn = item.subject.name;
                        }
                    }
                    subjectCollection.Add(subjects);
                }
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("PopulateSubjectCollectionAsync Error.");
                return false;
            }
        }

        /// <summary>
        /// 显示用户收视进度列表
        /// </summary>
        /// <param name="watchingListCollection"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static async Task<bool> PopulateWatchingListAsync(ObservableCollection<Watching> watchingListCollection, string username)
        {
            try
            {
                var watchingList = await GetWatchingListAsync(username);
                //清空原数据
                watchingListCollection.Clear();
                foreach (var watching in watchingList)
                {
                    if (string.IsNullOrEmpty(watching.subject.name_cn))
                    {
                        watching.subject.name_cn = watching.subject.name;
                    }
                    watchingListCollection.Add(watching);
                }
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("PopulateWatchingListAsync Error.");
                return false;
            }
        }

        /// <summary>
        /// 显示时间表
        /// </summary>
        /// <param name="bangumiCollection"></param>
        /// <returns></returns>
        public static async Task<bool> PopulateBangumiCalendarAsync(ObservableCollection<BangumiTimeLine> bangumiCollection)
        {
            try
            {
                var bangumiCalendarList = await GetBangumiCalendarAsync();
                //清空原数据
                bangumiCollection.Clear();
                foreach (var bangumiCalendar in bangumiCalendarList)
                {
                    foreach (var item in bangumiCalendar.items)
                    {
                        if (string.IsNullOrEmpty(item.name_cn))
                        {
                            item.name_cn = item.name;
                        }
                    }
                    bangumiCollection.Add(bangumiCalendar);
                }
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("PopulateBangumiCalendarAsync Error.");
                return false;
            }
        }

        /// <summary>
        /// 获取指定类别收藏信息
        /// </summary>
        /// <param name="username"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        private static async Task<SubjectCollection> GetSubjectCollectionAsync(string username, SubjectType subjectType)
        {
            string url = string.Format("{0}/user/{1}/collections/{2}?app_id={3}&max_results=25", BaseUrl, username, subjectType, Constants.client_id);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null)
                {
                    var result = JsonConvert.DeserializeObject<List<SubjectCollection>>(response);
                    return result.ElementAt(0);
                }
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetSubjectCollectionAsync Error.");
                return null;
            }
        }

        /// <summary>
        /// 获取指定条目收藏信息
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<CollectionStatus> GetCollectionStatusAsync(string subjectId)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthHelper.OAuthFile.access_token, true);
            string url = string.Format("{0}/collection/{1}?access_token={2}", BaseUrl, subjectId, token);

            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null)
                {
                    var result = JsonConvert.DeserializeObject<CollectionStatus>(response);
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetCollectionStatusAsync Error.");
                return null;
            }
        }

        /// <summary>
        /// 获取用户指定条目收视进度
        /// </summary>
        /// <param name="username"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Progress> GetProgressesAsync(string username, string subjectId)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthHelper.OAuthFile.access_token, true);
            string url = string.Format("{0}/user/{1}/progress?subject_id={2}&access_token={3}", BaseUrl, username, subjectId, token);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null)
                {
                    var result = JsonConvert.DeserializeObject<Progress>(response);
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetProgressesAsync Error.");
                return null;
            }
        }

        /// <summary>
        /// 获取用户收视进度
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private static async Task<List<Watching>> GetWatchingListAsync(string username)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthHelper.OAuthFile.access_token, true);
            string url = string.Format("{0}/user/{1}/collection?cat=watching&responseGroup=medium", BaseUrl, username);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null)
                {
                    // 反序列化指定名称的变量
                    JsonSerializerSettings jsonSerializerSetting = new JsonSerializerSettings();
                    jsonSerializerSetting.ContractResolver = new JsonPropertyContractResolver(new List<string> { "name", "subject_id", "ep_status", "subject", "name_cn", "images", "common", "eps_count" });
                    var result = JsonConvert.DeserializeObject<List<Watching>>(response, jsonSerializerSetting);
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetWatchingListAsync Error.");
                return null;
            }
        }

        /// <summary>
        /// 更新指定条目收藏状态
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatusEnum"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privace"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateCollectionStatusAsync(string subjectId, CollectionStatusEnum collectionStatusEnum,
            string comment = "", string rating = "", string privace = "0")
        {
            string token = await OAuthHelper.ReadFromFile(OAuthHelper.OAuthFile.access_token, true);
            string url = string.Format("{0}/collection/{1}/update?access_token={2}", BaseUrl, subjectId, token);
            string postData = "status=" + collectionStatusEnum.ToString();
            postData += "&comment=" + comment;
            postData += "&rating=" + rating;
            postData += "&privacy=" + privace;

            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                if (response != null && response.Contains(string.Format("\"type\":\"{0}\"", collectionStatusEnum.ToString())))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine("UpdateCollectionStatusAsync Error.");
                return false;
            }
        }

        /// <summary>
        /// 批量更新收视进度。
        /// 使用 HttpWebRequest 提交表单进行更新，更新收藏状态使用相同方法。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <param name="epsId"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressBatchAsync(int ep, EpStatusEnum status, string epsId)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthHelper.OAuthFile.access_token, true);
            string url = string.Format("{0}/ep/{1}/status/{2}?access_token={3}", BaseUrl, ep, status, token);
            string postData = "ep_id=" + epsId;

            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                if (response != null && response.Contains("\"error\":\"OK\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine("UpdateProgressBatchAsync Error.");
                return false;
            }
        }

        /// <summary>
        /// 更新收视进度
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressAsync(string ep, EpStatusEnum status)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthHelper.OAuthFile.access_token, true);
            string url = string.Format("{0}/ep/{1}/status/{2}?access_token={3}", BaseUrl, ep, status, token);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null && response.Contains("\"error\":\"OK\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine("UpdateProgressAsync Error.");
                return false;
            }
        }

        // ----------------- 获取信息，不涉及用户 ----------------------
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static async Task<SearchResult> GetSearchResultAsync(string keyWord, string type, int start, int n)
        {
            string url = string.Format("{0}/search/subject/{1}?type={2}&responseGroup=small&start={3}&max_results={4}", BaseUrl, keyWord, type, start, n);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null && !response.StartsWith("<!DOCTYPE html>"))
                {
                    var result = JsonConvert.DeserializeObject<SearchResult>(response);
                    if (result != null && result.list != null)
                    {
                        foreach (var item in result.list)
                        {
                            if (string.IsNullOrEmpty(item.name_cn))
                            {
                                item.name_cn = item.name;
                            }
                            if (item.images == null)
                            {
                                item.images = new Images { common = NoImageUri };
                            }
                        }
                        return result;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetSearchResultAsync Error.");
                return null;
            }
        }

        /// <summary>
        /// 获取指定条目所有剧集
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubjectEpsAsync(string subjectId)
        {
            string url = string.Format("{0}/subject/{1}/ep", BaseUrl, subjectId);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null)
                {
                    var result = JsonConvert.DeserializeObject<Subject>(response);
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetSubjectEpsAsync Error.");
                return null;
            }
        }

        /// <summary>
        /// 获取指定条目详情
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubjectAsync(string subjectId)
        {
            string url = string.Format("{0}/subject/{1}?responseGroup=large", BaseUrl, subjectId);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null)
                {
                    var result = JsonConvert.DeserializeObject<Subject>(response);
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetSubjectAsync Error.");
                return null;
            }
        }

        /// <summary>
        /// 获取时间表
        /// </summary>
        /// <returns></returns>
        private static async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            string url = string.Format("{0}/calendar", BaseUrl);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null)
                {
                    var result = JsonConvert.DeserializeObject<List<BangumiTimeLine>>(response);
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine("GetBangumiCalendarAsync Error.");
                return null;
            }
        }

        #region Enum
        public enum EpStatusEnum
        {
            watched,
            queue,
            drop,
            remove,
        }

        public enum CollectionStatusEnum
        {
            wish,
            collect,
            @do,
            on_hold,
            dropped,
        }

        public enum SubjectType : int
        {
            book = 1,
            anime = 2,
            music = 3,
            game = 4,
            real = 6,
        }
        #endregion

    }

    /// <summary>
    /// 重写Newtonsoft.Json的DefaultContractResolver类。
    /// 重写CreateProperties方法，反序列化指定名称的变量。
    /// </summary>
    public class JsonPropertyContractResolver : DefaultContractResolver
    {
        IEnumerable<string> lstInclude; public JsonPropertyContractResolver(IEnumerable<string> includeProperties)
        {
            lstInclude = includeProperties;
        }
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).ToList().FindAll(p => lstInclude.Contains(p.PropertyName));//需要输出的属性  
        }
    }


}
