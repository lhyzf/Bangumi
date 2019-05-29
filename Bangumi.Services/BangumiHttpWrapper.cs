using Bangumi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bangumi.Services
{
    public static class BangumiHttpWrapper
    {
        private const string BaseUrl = "https://api.bgm.tv";
        private const string client_id = Constants.ClientId;
        private const string NoImageUri = Constants.NoImageUri;

        /// <summary>
        /// 获取指定类别收藏信息。
        /// </summary>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public static async Task<Collection2> GetSubjectCollectionAsync(string userIdString, SubjectTypeEnum subjectType)
        {
            string url = string.Format("{0}/user/{1}/collections/{2}?app_id={3}&max_results=25", BaseUrl, userIdString, subjectType, client_id);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response == "null")
                {
                    return new Collection2 { Collects = new List<Collection>() };
                }
                var result = JsonConvert.DeserializeObject<List<Collection2>>(response);
                foreach (var type in result.ElementAt(0).Collects)
                {
                    foreach (var item in type.Items)
                    {
                        item.Subject.Name = System.Net.WebUtility.HtmlDecode(item.Subject.Name);
                        item.Subject.NameCn = string.IsNullOrEmpty(item.Subject.NameCn) ? item.Subject.Name : System.Net.WebUtility.HtmlDecode(item.Subject.NameCn);
                    }
                }
                return result.ElementAt(0);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectCollectionAsync Error.");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<SubjectStatus2> GetCollectionStatusAsync(string accessTokenString, string subjectId)
        {
            string url = string.Format("{0}/collection/{1}?access_token={2}", BaseUrl, subjectId, accessTokenString);

            try
            {
                string response = await HttpHelper.GetAsync(url);
                return JsonConvert.DeserializeObject<SubjectStatus2>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetCollectionStatusAsync Error.");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Progress> GetProgressesAsync(string userIdString, string accessTokenString, string subjectId)
        {
            string url = string.Format("{0}/user/{1}/progress?subject_id={2}&access_token={3}", BaseUrl, userIdString, subjectId, accessTokenString);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                return JsonConvert.DeserializeObject<Progress>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetProgressesAsync Error.");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 获取用户收视列表。
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Watching>> GetWatchingListAsync(string userIdString)
        {
            string url = string.Format("{0}/user/{1}/collection?cat=watching&responseGroup=medium", BaseUrl, userIdString);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                var result = JsonConvert.DeserializeObject<List<Watching>>(response);
                foreach (var watching in result)
                {
                    watching.Subject.Name = System.Net.WebUtility.HtmlDecode(watching.Subject.Name);
                    watching.Subject.NameCn = string.IsNullOrEmpty(watching.Subject.NameCn) ?
                        watching.Subject.Name : System.Net.WebUtility.HtmlDecode(watching.Subject.NameCn);
                    if (watching.Subject.Images == null)
                    {
                        watching.Subject.Images = new Images { Common = NoImageUri };
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetWatchingListAsync Error.");
                Debug.WriteLine(e.Message);
                throw e;
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
        public static async Task<bool> UpdateCollectionStatusAsync(string accessTokenString,
                                                                   string subjectId,
                                                                   CollectionStatusEnum collectionStatusEnum,
                                                                   string comment = "",
                                                                   string rating = "",
                                                                   string privace = "0")
        {
            string url = string.Format("{0}/collection/{1}/update?access_token={2}", BaseUrl, subjectId, accessTokenString);
            string postData = "status=" + collectionStatusEnum.ToString();
            postData += "&comment=" + comment;
            postData += "&rating=" + rating;
            postData += "&privacy=" + privace;

            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                if (response.Contains(string.Format("\"type\":\"{0}\"", collectionStatusEnum.ToString())))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateCollectionStatusAsync Error.");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 更新收视进度。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProgressAsync(string accessTokenString, string ep, EpStatusEnum status)
        {
            string url = string.Format("{0}/ep/{1}/status/{2}?access_token={3}", BaseUrl, ep, status, accessTokenString);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (response.Contains("\"error\":\"OK\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressAsync Error.");
                Debug.WriteLine(e.Message);
                throw e;
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
        public static async Task<bool> UpdateProgressBatchAsync(string accessTokenString, int ep, EpStatusEnum status, string epsId)
        {
            string url = string.Format("{0}/ep/{1}/status/{2}?access_token={3}", BaseUrl, ep, status, accessTokenString);
            string postData = "ep_id=" + epsId;

            try
            {
                string response = await HttpHelper.PostAsync(url, postData);
                if (response.Contains("\"error\":\"OK\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("UpdateProgressBatchAsync Error.");
                Debug.WriteLine(e.Message);
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
            string url = string.Format("{0}/subject/{1}/ep", BaseUrl, subjectId);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                return JsonConvert.DeserializeObject<Subject>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectEpsAsync Error.");
                Debug.WriteLine(e.Message);
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
            string url = string.Format("{0}/subject/{1}?responseGroup=large", BaseUrl, subjectId);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                var result = JsonConvert.DeserializeObject<Subject>(response);
                result.Name = System.Net.WebUtility.HtmlDecode(result.Name);
                result.NameCn = System.Net.WebUtility.HtmlDecode(result.NameCn);
                if (result.Eps != null)
                    foreach (var ep in result.Eps)
                    {
                        ep.NameCn = string.IsNullOrEmpty(ep.NameCn) ? ep.Name : ep.NameCn;
                    }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSubjectAsync Error.");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 获取时间表。
        /// </summary>
        /// <returns></returns>
        public static async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            string url = string.Format("{0}/calendar", BaseUrl);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                var result = JsonConvert.DeserializeObject<List<BangumiTimeLine>>(response);
                foreach (var bangumiCalendar in result)
                {
                    foreach (var item in bangumiCalendar.Items)
                    {
                        item.Name = System.Net.WebUtility.HtmlDecode(item.Name);
                        item.NameCn = string.IsNullOrEmpty(item.NameCn) ? item.Name : System.Net.WebUtility.HtmlDecode(item.NameCn);
                        if (item.Images == null)
                        {
                            item.Images = new Images { Common = NoImageUri };
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetBangumiCalendarAsync Error.");
                Debug.WriteLine(e.Message);
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
            string url = string.Format("{0}/search/subject/{1}?type={2}&responseGroup=small&start={3}&max_results={4}", BaseUrl, keyWord, type, start, n);
            try
            {
                string response = await HttpHelper.GetAsync(url);
                if (!response.StartsWith("<!DOCTYPE html>"))
                {
                    var result = JsonConvert.DeserializeObject<SearchResult>(response);
                    if (result != null && result.Results != null)
                    {
                        foreach (var item in result.Results)
                        {
                            item.Name = System.Net.WebUtility.HtmlDecode(item.Name);
                            item.NameCn = string.IsNullOrEmpty(item.NameCn) ? item.Name : System.Net.WebUtility.HtmlDecode(item.NameCn);
                            if (item.Images == null)
                            {
                                item.Images = new Images { Common = NoImageUri };
                            }
                        }
                        return result;
                    }
                }
                return new SearchResult { ResultCount = 0, Results = new List<Subject>() };
            }
            catch (Exception e)
            {
                Debug.WriteLine("GetSearchResultAsync Error.");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

    }
}
