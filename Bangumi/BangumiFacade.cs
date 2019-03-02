﻿using Bangumi.Models;
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

namespace Bangumi
{
    class BangumiFacade
    {
        private const string client_id = "bgm8905c514a1b94ec1";
        private const string client_secret = "b678c34dd896203627da308b6b453775";

        //处理 ObservableCollection 显示时间表
        public static async Task PopulateBangumiCalendarAsync(ObservableCollection<BangumiCalendar> bangumiCollection)
        {
            try
            {
                var bangumiCalendarList = await GetBangumiCalendarAsync();
                //清空原数据
                bangumiCollection.Clear();
                foreach (var bangumiCalendar in bangumiCalendarList)
                {
                    bangumiCollection.Add(bangumiCalendar);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        //获取时间表数据并反序列化
        private static async Task<List<BangumiCalendar>> GetBangumiCalendarAsync()
        {
            string url = "https://api.bgm.tv/calendar";
            HttpClient http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            var response = await http.GetAsync(url);
            var jsonMessage = await response.Content.ReadAsStringAsync();
            //string jsonMessage = "[{\"weekday\":{\"en\":\"Mon\",\"cn\":\"\u661f\u671f\u4e00\",\"ja\":\"\u6708\u8000\u65e5\",\"id\":1},\"items\":[{\"id\":212186,\"url\":\"http:// bgm.tv/subject/ 212186\",\"type\":2,\"name\":\"\u3051\u3082\u306e\u30d5\u30ec\u30f3\u30ba2\",\"name_cn\":\"\u517d\u5a18\u52a8\u7269\u56ed2\",\"summary\":\"\",\"air_date\":\"2019 - 01 - 14\",\"air_weekday\":1,\"rating\":{\"total\":127,\"score\":2.9},\"rank\":5164,\"images\":{\"large\":\"http://lain.bgm.tv/pic/cover/l/66/71/212186_VVyYd.jpg\",\"common\":\"http://lain.bgm.tv/pic/cover/c/66/71/212186_VVyYd.jpg\",\"medium\":\"http://lain.bgm.tv/pic/cover/m/66/71/212186_VVyYd.jpg\",\"small\":\"http://lain.bgm.tv/pic/cover/s/66/71/212186_VVyYd.jpg\",\"grid\":\"http://lain.bgm.tv/pic/cover/g/66/71/212186_VVyYd.jpg\"},\"collection\":{\"doing\":203}}]}]";
            //var serializer = new DataContractJsonSerializer(typeof(List<BangumiCalendar>));
            //var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonMessage));
            //var result = (List<BangumiCalendar>)serializer.ReadObject(ms);
            var result = JsonConvert.DeserializeObject<List<BangumiCalendar>>(jsonMessage);
            return result;
        }
    }
}
