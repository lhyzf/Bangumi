using Bangumi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Bangumi.Helper.OAuthHelper;

namespace Bangumi.Facades
{
    public class EpId
    {
        public string ep_id { get; set; }
    }

    class BangumiFacade
    {
        // 批量更新收视进度 20190315-无法批量更新
        public static async Task<bool> UpdateProgressBatchAsync(int ep, StatusEnum status, int n)
        {
            string epsId = "";
            for (int i = n; i > 0; i--)
            {
                epsId += (ep - i).ToString() +",";
            }
            epsId += ep.ToString();
            EpId postData = new EpId
            {
                ep_id = epsId,
            };
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/ep/{0}/status/{1}?access_token={2}", ep, status, token);
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8);
            var response = http.PostAsync(url, httpContent);
            var jsonMessage = await response.Result.Content.ReadAsStringAsync();
            var statusCode = response.Result.StatusCode;
            if (statusCode == System.Net.HttpStatusCode.OK)
                return true;
            return false;
        }

        // 更新收视进度
        public static async Task<bool> UpdateProgressAsync(string ep, StatusEnum status)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/ep/{0}/status/{1}?access_token={2}", ep, status, token);
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            try
            {
                var response = await http.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return true;
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        // 获取用户单个节目收视进度并反序列化
        public static async Task<Progress> GetProgressesAsync(string username, string subjectId)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/user/{0}/progress?subject_id={1}&access_token={2}", username, subjectId, token);
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            try
            {
                var response = await http.GetAsync(url);
                var jsonMessage = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Progress>(jsonMessage);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        // 显示用户收视进度列表
        public static async Task PopulateWatchingListAsync(ObservableCollection<Watching> watchingListCollection, string username)
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
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 获取用户收视进度并反序列化
        private static async Task<List<Watching>> GetWatchingListAsync(string username)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/user/{0}/collection?cat=watching&responseGroup=medium", username);
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            try
            {
                var response = await http.GetAsync(url);
                var jsonMessage = await response.Content.ReadAsStringAsync();
                // 反序列化指定名称的变量
                JsonSerializerSettings jsonSerializerSetting = new JsonSerializerSettings();
                jsonSerializerSetting.ContractResolver = new JsonPropertyContractResolver(new List<string> { "name", "subject_id", "ep_status", "subject", "name_cn", "images", "common" });
                var result = JsonConvert.DeserializeObject<List<Watching>>(jsonMessage, jsonSerializerSetting);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }


        // ----------------- 获取信息，不涉及用户 ----------------------
        // 获取剧集并反序列化
        public static async Task<Subject> GetSubjectEpsAsync(string subjectId)
        {
            string url = string.Format("https://api.bgm.tv/subject/{0}/ep", subjectId);
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            try
            {
                var response = await http.GetAsync(url);
                var jsonMessage = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Subject>(jsonMessage);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        // 获取详情并反序列化
        public static async Task<Subject> GetSubjectAsync(string subjectId)
        {
            string url = string.Format("https://api.bgm.tv/subject/{0}?responseGroup=small", subjectId);
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            try
            {
                var response = await http.GetAsync(url);
                var jsonMessage = await response.Content.ReadAsStringAsync();
                // 反序列化指定名称的变量
                JsonSerializerSettings jsonSerializerSetting = new JsonSerializerSettings();
                jsonSerializerSetting.ContractResolver = new JsonPropertyContractResolver(new List<string> { "name", "name_cn", "summary", "air_date", "air_weekday", "images", "common" });
                var result = JsonConvert.DeserializeObject<Subject>(jsonMessage, jsonSerializerSetting);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        // 显示时间表
        public static async Task PopulateBangumiCalendarAsync(ObservableCollection<BangumiTimeLine> bangumiCollection)
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
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 获取时间表数据并反序列化
        private static async Task<List<BangumiTimeLine>> GetBangumiCalendarAsync()
        {
            string url = "https://api.bgm.tv/calendar";
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            try
            {
                var response = await http.GetAsync(url);
                var jsonMessage = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<BangumiTimeLine>>(jsonMessage);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }
        public enum StatusEnum
        {
            watched,
            queue,
            drop,
            remove,
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
