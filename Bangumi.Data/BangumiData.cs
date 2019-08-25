using AngleSharp.Html.Parser;
using Bangumi.Data.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bangumi.Data
{
    public static class BangumiData
    {
        private static HtmlParser htmlParser;
        private static BangumiDataSet dataSet;
        private static Dictionary<string, string> seasonIdMap;
        private static string latestVersion;
        private static string folderPath;

        public static string Version { get; private set; }
        public static bool UseBiliApp { get; set; }

        /// <summary>
        /// 读取文件，将数据加载到内存
        /// </summary>
        /// <param name="datafolderpath">文件夹路径</param>
        public static async Task Init(string datafolderpath, bool useBiliApp = false)
        {
            folderPath = datafolderpath;
            UseBiliApp = useBiliApp;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (dataSet == null &&
                File.Exists(folderPath + "\\data.json") &&
                File.Exists(folderPath + "\\version"))
            {
                dataSet = JsonConvert.DeserializeObject<BangumiDataSet>(await FileHelper.ReadTextAsync(folderPath + "\\data.json"));
                Version = await FileHelper.ReadTextAsync(folderPath + "\\version");
            }
            if (seasonIdMap == null)
            {
                if (File.Exists(folderPath + "\\map.json"))
                {
                    seasonIdMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(await FileHelper.ReadTextAsync(folderPath + "\\map.json"));
                }
                else
                {
                    seasonIdMap = new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// 解析网页获取最新版本号，并暂存
        /// </summary>
        /// <returns>返回最新版本号</returns>
        public static async Task<string> GetLatestVersion()
        {
            try
            {
                if (htmlParser == null)
                {
                    htmlParser = new HtmlParser();
                }
                // 通过URL获取HTML
                var htmlDoc = await HTTPHelper.GetTextByUrlAsync("https://github.com/bangumi-data/bangumi-data/releases");
                // HTML 解析成 IHtmlDocument
                var dom = htmlParser.ParseDocument(htmlDoc);
                // 查找第一个release
                var release = dom.QuerySelector("div.release-entry");
                if (release != null)
                {
                    // 查找链接
                    var ss = release.QuerySelectorAll("a").Where(a => a.GetAttribute("href").Contains("/bangumi-data/bangumi-data/releases/tag/")).ToList();
                    latestVersion = ss[0].GetAttribute("href").Replace("/bangumi-data/bangumi-data/releases/tag/", "");
                    return latestVersion;
                }
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        /// <summary>
        /// 下载最新的数据，并更新原有数据，不比较版本号
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> DownloadLatestBangumiData()
        {
            //var data = await HTTPHelper.GetTextByUrlAsync("https://github.com/bangumi-data/bangumi-data/raw/master/dist/data.json");
            var data = await HTTPHelper.GetTextByUrlAsync("https://cdn.jsdelivr.net/npm/bangumi-data@0.3/dist/data.json");
            if (string.IsNullOrEmpty(latestVersion))
                await GetLatestVersion();
            if (!string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(latestVersion))
            {
                try
                {
                    dataSet = JsonConvert.DeserializeObject<BangumiDataSet>(data);
                    await FileHelper.WriteTextAsync(folderPath + "\\data.json", data);
                    Version = latestVersion;
                    await FileHelper.WriteTextAsync(folderPath + "\\version", Version);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据Bangumi的ID返回所有放送网站
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<Site>> GetAirSitesByBangumiIdAsync(string id)
        {
            if (dataSet == null)
            {
                return new List<Site>();
            }

            var siteList = dataSet.Items.Where(e => e.Sites.Where(s => s.SiteName == "bangumi" && s.Id == id).Count() != 0)
                                          .FirstOrDefault()?.Sites
                                          .Where(s => dataSet.SiteMeta[s.SiteName].Type == "onair").ToList();
            if (siteList != null)
            {
                foreach (var site in siteList)
                {
                    site.Url = string.IsNullOrEmpty(site.Id) ?
                               site.Url :
                               dataSet.SiteMeta[site.SiteName].UrlTemplate.Replace("{{id}}", site.Id);
                }
                // 启用设置，将mediaid转换为seasonid
                if (UseBiliApp)
                {
                    var biliSite = siteList.Where(s => s.SiteName == "bilibili").FirstOrDefault();
                    if (biliSite != null)
                    {
                        string seasonId;
                        if (!seasonIdMap.TryGetValue(biliSite.Id, out seasonId))
                        {
                            var url = string.Format("https://bangumi.bilibili.com/view/web_api/media?media_id={0}", biliSite.Id);
                            try
                            {
                                var result = await HTTPHelper.GetTextByUrlAsync(url);
                                JObject jObject = JObject.Parse(result);
                                seasonId = jObject.SelectToken("result.param.season_id").ToString();
                                seasonIdMap.Add(biliSite.Id, seasonId);
                                _ = FileHelper.WriteTextAsync(folderPath + "\\map.json", JsonConvert.SerializeObject(seasonIdMap));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("获取seasonId失败");
                                Console.WriteLine(e.Message);
                                return siteList;
                            }
                        }
                        biliSite.Url = "bilibili://bangumi/season/" + seasonId;
                    }
                }
                return siteList;
            }
            else
            {
                return new List<Site>();
            }
        }


    }
}
