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
using static Bangumi.Helper.OAuthHelper;

namespace Bangumi.Facades
{
    class BangumiFacade
    {
        private static string NoImageUri = "ms-appx:///Assets/NoImage.png";

        // 处理收藏信息
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

        // 获取指定类别收藏信息并反序列化
        private static async Task<SubjectCollection> GetSubjectCollectionAsync(string username, SubjectType subjectType)
        {
            string url = string.Format("https://api.bgm.tv/user/{0}/collections/{1}?app_id={2}&max_results=25", username, subjectType, client_id);
            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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

        // 更新指定条目收藏状态
        public static async Task<bool> UpdateCollectionStatusAsync(string subjectId, CollectionStatusEnum collectionStatusEnum,
            string comment = "", string rating = "", string privace = "0")
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/collection/{0}/update?access_token={1}", subjectId, token);
            string postData = "status=" + collectionStatusEnum.ToString();
            postData += "&comment=" + comment;
            postData += "&rating=" + rating;
            postData += "&privacy=" + privace;

            try
            {
                string response = await HttpWebRequestHelper.PostResponseAsync(url, postData);
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

        // 获取指定条目收藏信息
        public static async Task<CollectionStatus> GetCollectionStatusAsync(string subjectId)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/collection/{0}?access_token={1}", subjectId, token);

            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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

        // 批量更新收视进度
        // 使用 HttpWebRequest 提交表单进行更新，更新收藏状态使用相同方法
        public static async Task<bool> UpdateProgressBatchAsync(int ep, EpStatusEnum status, string epsId)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/ep/{0}/status/{1}?access_token={2}&ep_id={3}", ep, status, token, epsId);
            string postData = "ep_id=" + epsId;

            try
            {
                string response = await HttpWebRequestHelper.PostResponseAsync(url, postData);
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

        // 更新收视进度
        public static async Task<bool> UpdateProgressAsync(string ep, EpStatusEnum status)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/ep/{0}/status/{1}?access_token={2}", ep, status, token);
            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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

        // 获取用户单个节目收视进度并反序列化
        public static async Task<Progress> GetProgressesAsync(string username, string subjectId)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/user/{0}/progress?subject_id={1}&access_token={2}", username, subjectId, token);
            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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

        // 显示用户收视进度列表
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

        // 获取用户收视进度并反序列化
        private static async Task<List<Watching>> GetWatchingListAsync(string username)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/user/{0}/collection?cat=watching&responseGroup=medium", username);
            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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


        // ----------------- 获取信息，不涉及用户 ----------------------
        // 搜索
        public static async Task<SearchResult> GetSearchResultAsync(string keyWord, string type, int start, int n)
        {
            string url = string.Format("https://api.bgm.tv/search/subject/{0}?type={1}&responseGroup=small&start={2}&max_results={3}", keyWord, type, start, n);
            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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
        
        // 获取剧集并反序列化
        public static async Task<Subject> GetSubjectEpsAsync(string subjectId)
        {
            string url = string.Format("https://api.bgm.tv/subject/{0}/ep", subjectId);
            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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

        // 获取详情并反序列化
        public static async Task<Subject> GetSubjectAsync(string subjectId)
        {
            string url = string.Format("https://api.bgm.tv/subject/{0}?responseGroup=large", subjectId);
            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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

        // 处理时间表
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

        // 获取时间表数据并反序列化
        private static async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            string url = "https://api.bgm.tv/calendar";
            try
            {
                string response = await HttpWebRequestHelper.GetResponseAsync(url);
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

    }

    // 重写Newtonsoft.Json的DefaultContractResolver类。
    // 重写CreateProperties方法，反序列化指定名称的变量。
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
