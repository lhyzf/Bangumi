using Bangumi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using static Bangumi.Helper.OAuthHelper;

namespace Bangumi.Facades
{
    class BangumiFacade
    {

        // 处理 ObservableCollection 显示收视进度列表
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
                var result = JsonConvert.DeserializeObject<List<Watching>>(jsonMessage);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        // 处理 ObservableCollection 显示某一节目收视进度
        public static async Task PopulateProgressAsync(ObservableCollection<Watching> watchingListCollection, string username, string subjectId)
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

        // 获取用户单个节目收视进度并反序列化
        private static async Task<List<Progress>> GetProgressesAsync(string username)
        {
            string token = await Helper.OAuthHelper.ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://api.bgm.tv/user/{0}/progress?access_token={1}", username, token);
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            try
            {
                var response = await http.GetAsync(url);
                var jsonMessage = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<Progress>>(jsonMessage);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        // 处理 ObservableCollection 显示时间表
        public static async Task PopulateBangumiCalendarAsync(ObservableCollection<BangumiCalendar> bangumiCollection)
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
        private static async Task<List<BangumiCalendar>> GetBangumiCalendarAsync()
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
                //string jsonMessage = "[{\"weekday\":{\"en\":\"Mon\",\"cn\":\"\u661f\u671f\u4e00\",\"ja\":\"\u6708\u8000\u65e5\",\"id\":1},\"items\":[{\"id\":212186,\"url\":\"http:// bgm.tv/subject/ 212186\",\"type\":2,\"name\":\"\u3051\u3082\u306e\u30d5\u30ec\u30f3\u30ba2\",\"name_cn\":\"\u517d\u5a18\u52a8\u7269\u56ed2\",\"summary\":\"\",\"air_date\":\"2019 - 01 - 14\",\"air_weekday\":1,\"rating\":{\"total\":127,\"score\":2.9},\"rank\":5164,\"images\":{\"large\":\"http://lain.bgm.tv/pic/cover/l/66/71/212186_VVyYd.jpg\",\"common\":\"http://lain.bgm.tv/pic/cover/c/66/71/212186_VVyYd.jpg\",\"medium\":\"http://lain.bgm.tv/pic/cover/m/66/71/212186_VVyYd.jpg\",\"small\":\"http://lain.bgm.tv/pic/cover/s/66/71/212186_VVyYd.jpg\",\"grid\":\"http://lain.bgm.tv/pic/cover/g/66/71/212186_VVyYd.jpg\"},\"collection\":{\"doing\":203}}]}]";
                //var serializer = new DataContractJsonSerializer(typeof(List<BangumiCalendar>));
                //var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonMessage));
                //var result = (List<BangumiCalendar>)serializer.ReadObject(ms);
                var result = JsonConvert.DeserializeObject<List<BangumiCalendar>>(jsonMessage);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }
    }
}
