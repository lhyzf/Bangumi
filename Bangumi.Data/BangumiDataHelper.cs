using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;

namespace Bangumi.Data
{
    public static class BangumiDataHelper
    {
        private static HtmlParser htmlParser = new HtmlParser();
        private static BangumiData bangumiData;
        private static string version;
        private static string latestVersion;

        /// <summary>
        /// 读取文件，将数据加载到内存
        /// </summary>
        /// <param name="datafolderpath">文件夹路径</param>
        public static void InitBangumiData(string datafolderpath)
        {
            if (bangumiData == null && File.Exists(datafolderpath + "\\data.json") && File.Exists(datafolderpath + "\\version"))
            {
                bangumiData = JsonConvert.DeserializeObject<BangumiData>(File.ReadAllText(datafolderpath + "\\data.json"));
                version = File.ReadAllText(datafolderpath + "\\version");
            }
        }

        /// <summary>
        /// 获取最新版本号，并暂存
        /// </summary>
        /// <returns>返回最新版本号</returns>
        public static async Task<string> GetLatestVersion()
        {
            try
            {
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
        /// <param name="datafolderpath">存在的文件夹路径</param>
        /// <returns></returns>
        public static async Task<bool> DownloadLatestBangumiData(string datafolderpath)
        {
            var data = await HTTPHelper.GetTextByUrlAsync("https://github.com/bangumi-data/bangumi-data/raw/master/dist/data.json");
            if (string.IsNullOrEmpty(latestVersion))
                await GetLatestVersion();
            if (!string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(latestVersion))
            {
                try
                {
                    bangumiData = JsonConvert.DeserializeObject<BangumiData>(data);
                    File.WriteAllText(datafolderpath + "\\data.json", data);
                    version = latestVersion;
                    File.WriteAllText(datafolderpath + "\\version", version);
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
        /// 获取所有数据
        /// </summary>
        /// <returns></returns>
        public static BangumiData GetAllBangumiData()
        {
            return bangumiData;
        }

        public static string GetCurVersion()
        {
            return version;
        }

        /// <summary>
        /// 根据Bangumi的ID返回所有放送网站
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<Site> GetAirSitesByBangumiID(string id)
        {
            if (bangumiData == null)
            {
                return new List<Site>();
            }

            var siteList = bangumiData.Items.Where(e => e.Sites.Where(s => s.SiteName == "bangumi" && s.Id == id).Count() != 0)
                                          .FirstOrDefault()?.Sites
                                          .Where(s => bangumiData.SiteMeta[s.SiteName].Type == "onair").ToList();
            if (siteList != null)
            {
                foreach (var site in siteList)
                {
                    site.Url = string.IsNullOrEmpty(site.Id) ?
                               site.Url :
                               bangumiData.SiteMeta[site.SiteName].UrlTemplate.Replace("{{id}}", site.Id);
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
