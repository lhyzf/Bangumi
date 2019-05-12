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
    public class BangumiFacade
    {
        private const string baseUrl = Constants.baseUrl;
        private const string client_id = Constants.client_id;
        private const string NoImageUri = Constants.noImageUri;

        /// <summary>
        /// 显示用户选定类别收藏信息。
        /// </summary>
        /// <param name="subjectCollection"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public static async Task<bool> PopulateSubjectCollectionAsync(ObservableCollection<Collect> subjectCollection, SubjectType subjectType)
        {
            try
            {
                //清空原数据
                subjectCollection.Clear();

                //从文件反序列化
                var PreCollection = JsonConvert.DeserializeObject<List<Collect>>(await FileHelper.ReadFromTempFile(subjectType + "temp"));
                if (PreCollection != null)
                {
                    foreach (var type in PreCollection)
                    {
                        subjectCollection.Add(type);
                    }
                }

                var subjectCollections = await GetSubjectCollectionAsync(subjectType);
                if (subjectCollections != null && subjectCollections.collects != null && !subjectCollection.Equals(subjectCollections.collects))
                {
                    //清空原数据
                    subjectCollection.Clear();
                    foreach (var type in subjectCollections.collects)
                    {
                        foreach (var item in type.list)
                        {
                            if (string.IsNullOrEmpty(item.subject.name_cn))
                            {
                                item.subject.name_cn = item.subject.name;
                            }
                        }
                        subjectCollection.Add(type);
                    }
                }

                //将对象序列化并存储到文件
                await FileHelper.WriteToTempFile(JsonConvert.SerializeObject(subjectCollection), subjectType + "temp");

                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("PopulateSubjectCollectionAsync Error.");
                return false;
            }
        }

        /// <summary>
        /// 显示用户收视进度列表。
        /// </summary>
        /// <param name="watchingListCollection"></param>
        /// <returns></returns>
        public static async Task<bool> PopulateWatchingListAsync(ObservableCollection<WatchingStatus> watchingListCollection)
        {
            try
            {
                //从文件反序列化
                var PreWatchings = JsonConvert.DeserializeObject<List<WatchingStatus>>(await FileHelper.ReadFromTempFile("hometemp"));
                if (PreWatchings != null)
                {
                    foreach (var sub in PreWatchings)
                    {
                        if (watchingListCollection.Where(e => e.subject_id == sub.subject_id).Count() == 0)
                            watchingListCollection.Add(sub);
                    }
                }

                var watchingList = await GetWatchingListAsync();

                var deletedItems = new List<WatchingStatus>(); //标记要删除的条目
                foreach (var sub in watchingListCollection)
                {
                    //根据最新的进度删除原有条目
                    if (watchingList.Where(e => e.subject_id == sub.subject_id).Count() == 0)
                        deletedItems.Add(sub);
                }
                foreach (var item in deletedItems) //删除条目
                {
                    watchingListCollection.Remove(item);
                }

                foreach (var watching in watchingList)
                {
                    //若在看含有原来没有的条目则新增
                    var item = watchingListCollection.Where(e => e.subject_id == watching.subject_id).FirstOrDefault();
                    if (item == null)
                    {
                        WatchingStatus watchingStatus = new WatchingStatus();
                        if (string.IsNullOrEmpty(watching.subject.name_cn))
                        {
                            watching.subject.name_cn = watching.subject.name;
                        }
                        if (watching.subject.images == null)
                        {
                            watching.subject.images = new Images { common = NoImageUri };
                        }

                        watchingStatus.name = watching.subject.name;
                        watchingStatus.name_cn = watching.subject.name_cn;
                        watchingStatus.image = watching.subject.images.common;
                        watchingStatus.subject_id = watching.subject_id;
                        watchingStatus.lasttouch = watching.lasttouch;
                        watchingStatus.url = watching.subject.url;

                        //获取EP详细信息
                        var subject = await GetSubjectEpsAsync(watchingStatus.subject_id.ToString());
                        var progress = await GetProgressesAsync(watchingStatus.subject_id.ToString());

                        watchingStatus.eps = new List<SimpleEp>();
                        foreach (var ep in subject.eps)
                        {
                            SimpleEp simpleEp = new SimpleEp();
                            simpleEp.id = ep.id;
                            simpleEp.sort = ep.sort;
                            simpleEp.status = ep.status;
                            simpleEp.type = ep.type;
                            watchingStatus.eps.Add(simpleEp);
                        }
                        if (watchingStatus.eps.Where(e => e.status == "NA").Count() == 0)
                            watchingStatus.eps_count = "全" + watchingStatus.eps.Count + "话";
                        else
                            watchingStatus.eps_count = "更新到第" + (watchingStatus.eps.Count - watchingStatus.eps.Where(e => e.status == "NA").Count()) + "话";
                        if (progress != null)
                        {
                            watchingStatus.watched_eps = "看到第" + progress.eps.Count + "话";
                            if (progress.eps.Count < (watchingStatus.eps.Count - watchingStatus.eps.Where(e => e.status == "NA").Count()))
                                watchingStatus.ep_color = "#d26585";
                            else
                                watchingStatus.ep_color = "gray";
                        }
                        else
                        {
                            watchingStatus.watched_eps = "尚未观看";
                            watchingStatus.ep_color = "#d26585";
                        }
                        watchingListCollection.Add(watchingStatus);
                    }
                    else
                    {
                        if (watching.subject.eps_count != 0 && item.eps.Count != watching.subject.eps_count)
                        {
                            var subject = await GetSubjectEpsAsync(item.subject_id.ToString());
                            item.eps.Clear();
                            foreach (var ep in subject.eps)
                            {
                                SimpleEp simpleEp = new SimpleEp();
                                simpleEp.id = ep.id;
                                simpleEp.sort = ep.sort;
                                simpleEp.status = ep.status;
                                simpleEp.type = ep.type;
                                item.eps.Add(simpleEp);
                            }
                            if (item.eps.Where(e => e.status == "NA").Count() == 0)
                                item.eps_count = "全" + item.eps.Count + "话";
                            else
                                item.eps_count = "更新到第" + (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()) + "话";

                        }
                        if (item.lasttouch != watching.lasttouch)
                        {
                            var progress = await GetProgressesAsync(item.subject_id.ToString());
                            if (progress != null)
                            {
                                item.watched_eps = "看到第" + progress.eps.Count + "话";
                                if (progress.eps.Count < (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()))
                                    item.ep_color = "#d26585";
                                else
                                    item.ep_color = "gray";
                            }
                            else
                            {
                                item.watched_eps = "尚未观看";
                                item.ep_color = "#d26585";
                            }
                            item.lasttouch = watching.lasttouch;
                        }
                    }
                }

                //将对象序列化并存储到文件
                await FileHelper.WriteToTempFile(JsonConvert.SerializeObject(watchingListCollection), "hometemp");

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("PopulateWatchingListAsync Error.");
                Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// 显示时间表。
        /// </summary>
        /// <param name="bangumiCollection"></param>
        /// <returns></returns>
        public static async Task<bool> PopulateBangumiCalendarAsync(ObservableCollection<BangumiTimeLine> bangumiCollection)
        {
            try
            {
                //从文件反序列化
                var PreCalendar = JsonConvert.DeserializeObject<List<BangumiTimeLine>>(await FileHelper.ReadFromTempFile("calendartemp"));
                if (PreCalendar != null)
                {
                    foreach (var item in PreCalendar)
                    {
                        bangumiCollection.Add(item);
                    }
                }

                var bangumiCalendarList = await GetBangumiCalendarAsync();
                //清空原数据
                bangumiCollection.Clear();
                int day = GetDayOfWeek();
                foreach (var bangumiCalendar in bangumiCalendarList)
                {
                    foreach (var item in bangumiCalendar.items)
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
                    if (bangumiCalendar.weekday.id <= day)
                    {
                        bangumiCollection.Add(bangumiCalendar);
                    }
                    else
                    {
                        bangumiCollection.Insert(bangumiCollection.Count - day, bangumiCalendar);
                    }
                }

                //将对象序列化并存储到文件
                await FileHelper.WriteToTempFile(JsonConvert.SerializeObject(bangumiCollection), "calendartemp");

                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("PopulateBangumiCalendarAsync Error.");
                return false;
            }
        }

        /// <summary>
        /// 获取指定类别收藏信息。
        /// </summary>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        private static async Task<SubjectCollection> GetSubjectCollectionAsync(SubjectType subjectType)
        {
            string url = string.Format("{0}/user/{1}/collections/{2}?app_id={3}&max_results=25", baseUrl, OAuthHelper.UserIdString, subjectType, client_id);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response == "null")
                {
                    return new SubjectCollection();
                }
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
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<CollectionStatus> GetCollectionStatusAsync(string subjectId)
        {
            string token = OAuthHelper.AccessTokenString;
            string url = string.Format("{0}/collection/{1}?access_token={2}", baseUrl, subjectId, token);

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
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Progress> GetProgressesAsync(string subjectId)
        {
            string token = OAuthHelper.AccessTokenString;
            string url = string.Format("{0}/user/{1}/progress?subject_id={2}&access_token={3}", baseUrl, OAuthHelper.UserIdString, subjectId, token);
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
        /// 获取用户收视进度。
        /// </summary>
        /// <returns></returns>
        private static async Task<List<Watching>> GetWatchingListAsync()
        {
            string token = OAuthHelper.AccessTokenString;
            string url = string.Format("{0}/user/{1}/collection?cat=watching&responseGroup=medium", baseUrl, OAuthHelper.UserIdString);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response != null)
                {
                    // 反序列化指定名称的变量
                    JsonSerializerSettings jsonSerializerSetting = new JsonSerializerSettings();
                    jsonSerializerSetting.ContractResolver = new JsonPropertyContractResolver(new List<string> { "name", "subject_id", "ep_status", "subject", "name_cn", "images", "common", "eps_count", "lasttouch" });
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
        /// 更新指定条目收藏状态。
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
            string token = OAuthHelper.AccessTokenString;
            string url = string.Format("{0}/collection/{1}/update?access_token={2}", baseUrl, subjectId, token);
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
            string token = OAuthHelper.AccessTokenString;
            string url = string.Format("{0}/ep/{1}/status/{2}?access_token={3}", baseUrl, ep, status, token);
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
        /// 更新收视进度。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressAsync(string ep, EpStatusEnum status)
        {
            string token = OAuthHelper.AccessTokenString;
            string url = string.Format("{0}/ep/{1}/status/{2}?access_token={3}", baseUrl, ep, status, token);
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
        /// 搜索。
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static async Task<SearchResult> GetSearchResultAsync(string keyWord, string type, int start, int n)
        {
            string url = string.Format("{0}/search/subject/{1}?type={2}&responseGroup=small&start={3}&max_results={4}", baseUrl, keyWord, type, start, n);
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
        /// 获取指定条目所有章节。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubjectEpsAsync(string subjectId)
        {
            string url = string.Format("{0}/subject/{1}/ep", baseUrl, subjectId);
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
        /// 获取指定条目详情。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Subject> GetSubjectAsync(string subjectId)
        {
            string url = string.Format("{0}/subject/{1}?responseGroup=large", baseUrl, subjectId);
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
        /// 获取时间表。
        /// </summary>
        /// <returns></returns>
        private static async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            string url = string.Format("{0}/calendar", baseUrl);
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

        private static int GetDayOfWeek()
        {
            int day = 0;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    day = 0;
                    break;
                case DayOfWeek.Tuesday:
                    day = 1;
                    break;
                case DayOfWeek.Wednesday:
                    day = 2;
                    break;
                case DayOfWeek.Thursday:
                    day = 3;
                    break;
                case DayOfWeek.Friday:
                    day = 4;
                    break;
                case DayOfWeek.Saturday:
                    day = 5;
                    break;
                case DayOfWeek.Sunday:
                    day = 6;
                    break;
                default:
                    break;
            }
            return day;
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
            no,
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
