using Bangumi.Api.Models;
using Bangumi.Api.Utils;
using Bangumi.Api.Services;
using Bangumi.Helper;
using Bangumi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bangumi.Facades
{
    public static class BangumiFacade
    {
        /// <summary>
        /// 显示用户收视进度列表。
        /// </summary>
        /// <param name="watchingListCollection"></param>
        /// <returns></returns>
        public static async Task PopulateWatchingListAsync(ObservableCollection<WatchingStatus> watchingListCollection)
        {
            try
            {
                //从文件反序列化
                var PreWatchings = JsonConvert.DeserializeObject<List<WatchingStatus>>(await FileHelper.ReadFromCacheFileAsync("JsonCache\\home"));
                if (PreWatchings != null)
                {
                    foreach (var sub in PreWatchings)
                    {
                        // 将Collection中没有的添加进去
                        if (watchingListCollection.Where(e => e.subject_id == sub.subject_id).Count() == 0)
                            watchingListCollection.Add(sub);
                    }
                }
                //else
                //{
                //    watchingListCollection.Clear();
                //}

                var watchingList = await BangumiHttpWrapper.GetWatchingListAsync(OAuthHelper.MyToken.UserId);

                var deletedItems = new List<WatchingStatus>(); //标记要删除的条目
                foreach (var sub in watchingListCollection)
                {
                    //根据最新的进度删除原有条目
                    if (watchingList.Where(e => e.SubjectId == sub.subject_id).Count() == 0)
                        deletedItems.Add(sub);
                }
                foreach (var item in deletedItems) //删除条目
                {
                    watchingListCollection.Remove(item);
                }

                foreach (var watching in watchingList)
                {
                    //若在看含有原来没有的条目则新增,之后再细化
                    var item = watchingListCollection.Where(e => e.subject_id == watching.SubjectId).FirstOrDefault();
                    if (item == null)
                    {
                        WatchingStatus watchingStatus = new WatchingStatus();
                        watchingStatus.name = watching.Subject.Name;
                        watchingStatus.name_cn = watching.Subject.NameCn;
                        watchingStatus.image = watching.Subject.Images.Common;
                        watchingStatus.subject_id = watching.SubjectId;
                        watchingStatus.url = watching.Subject.Url;
                        watchingStatus.ep_color = "Gray";
                        watchingStatus.lasttouch = 0;
                        watchingStatus.watched_eps = watching.EpStatus;
                        watchingStatus.updated_eps = watching.Subject.EpsCount.ToString();

                        watchingListCollection.Add(watchingStatus);
                    }
                }
                foreach (var watching in watchingList)
                {
                    var item = watchingListCollection.Where(e => e.subject_id == watching.SubjectId).FirstOrDefault();
                    if (item != null)
                    {
                        item.isUpdating = true;
                        // 首次更新
                        if (item.lasttouch == 0)
                        {
                            // 获取EP详细信息
                            var subject = await GetSubjectAsync(item.subject_id.ToString());
                            var progress = await GetProgressesAsync(item.subject_id.ToString());

                            item.eps = new List<SimpleEp>();
                            foreach (var ep in subject.Eps)
                            {
                                SimpleEp simpleEp = new SimpleEp();
                                simpleEp.id = ep.Id;
                                simpleEp.sort = ep.Sort;
                                simpleEp.status = ep.Status;
                                simpleEp.type = ep.Type;
                                simpleEp.name = ep.NameCn == "" ? ep.Name : ep.NameCn;
                                item.eps.Add(simpleEp);
                            }
                            if (item.eps.Where(e => e.status == "NA").Count() == 0)
                                item.updated_eps = "全" + item.eps.Count + "话";
                            else
                                item.updated_eps = "更新到第" + (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()) + "话";
                            if (progress != null)
                            {
                                item.watched_eps = progress.Eps.Count;
                                if (item.eps.Count == item.watched_eps)
                                    item.next_ep = 0;
                                if (progress.Eps.Count < (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()))
                                    item.ep_color = "#d26585";
                                else
                                    item.ep_color = "Gray";
                                foreach (var ep in item.eps) // 填充用户观看状态
                                {
                                    foreach (var p in progress.Eps)
                                    {
                                        if (p.Id == ep.id)
                                        {
                                            ep.status = p.Status.CnName;
                                            progress.Eps.Remove(p);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                item.watched_eps = 0;
                                item.ep_color = "#d26585";
                            }
                            item.next_ep = item.eps.Where(ep => ep.status == "Air" || ep.status == "Today" || ep.status == "NA").FirstOrDefault().sort;
                            item.lasttouch = watching.LastTouch;
                            item.lastupdate = DateTime.Today.ConvertDateTimeToJsTick();
                        }
                        else
                        {
                            // 对条目有 修改 或 当天首次加载 进行更新
                            if (item.lasttouch != watching.LastTouch
                                || item.lastupdate != DateTime.Today.ConvertDateTimeToJsTick())
                            {
                                var subject = await BangumiHttpWrapper.GetSubjectEpsAsync(item.subject_id.ToString());
                                item.eps.Clear();
                                Console.WriteLine(watching.LastTouch);
                                foreach (var ep in subject.Eps)
                                {
                                    SimpleEp simpleEp = new SimpleEp();
                                    simpleEp.id = ep.Id;
                                    simpleEp.sort = ep.Sort;
                                    simpleEp.status = ep.Status;
                                    simpleEp.type = ep.Type;
                                    simpleEp.name = ep.NameCn;
                                    item.eps.Add(simpleEp);
                                }
                                if (item.eps.Where(e => e.status == "NA").Count() == 0)
                                    item.updated_eps = "全" + item.eps.Count + "话";
                                else
                                    item.updated_eps = "更新到第" + (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()) + "话";

                                var progress = await GetProgressesAsync(item.subject_id.ToString());
                                if (progress != null)
                                {
                                    item.watched_eps = progress.Eps.Count;
                                    if (item.eps.Count == item.watched_eps)
                                        item.next_ep = 0;
                                    if (item.watched_eps < (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()))
                                        item.ep_color = "#d26585";
                                    else
                                        item.ep_color = "Gray";
                                    foreach (var ep in item.eps) //用户观看状态
                                    {
                                        foreach (var p in progress.Eps)
                                        {
                                            if (p.Id == ep.id)
                                            {
                                                ep.status = p.Status.CnName;
                                                progress.Eps.Remove(p);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    item.watched_eps = 0;
                                    item.ep_color = "#d26585";
                                }
                                item.next_ep = item.eps.Where(ep => ep.status == "Air" || ep.status == "Today" || ep.status == "NA").FirstOrDefault().sort;
                                item.lasttouch = watching.LastTouch;
                                item.lastupdate = DateTime.Today.ConvertDateTimeToJsTick();
                            }
                        }
                        item.isUpdating = false;
                    }
                }
            }
            catch (Exception e)
            {
                foreach (var item in watchingListCollection.Where(i => i.isUpdating == true))
                {
                    item.isUpdating = false;
                }
                Debug.WriteLine("显示用户收视进度列表失败。");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 显示用户选定类别收藏信息。
        /// </summary>
        /// <param name="subjectCollection"></param>
        /// <param name="subjectType"></param>
        /// <returns></returns>
        public static async Task PopulateSubjectCollectionAsync(ObservableCollection<Collection> subjectCollection, SubjectTypeEnum subjectType)
        {
            try
            {
                //从文件反序列化
                var PreCollection = JsonConvert.DeserializeObject<List<Collection>>(await FileHelper.ReadFromCacheFileAsync("JsonCache\\" + subjectType));
                subjectCollection.Clear();
                if (PreCollection != null)
                {
                    foreach (var type in PreCollection)
                    {
                        subjectCollection.Add(type);
                    }
                }

                var subjectCollections = await BangumiHttpWrapper.GetSubjectCollectionAsync(OAuthHelper.MyToken.UserId, subjectType);

                //清空原数据
                subjectCollection.Clear();
                foreach (var type in subjectCollections.Collects)
                {
                    subjectCollection.Add(type);
                }

                //将对象序列化并存储到文件
                await FileHelper.WriteToCacheFileAsync(JsonConvert.SerializeObject(subjectCollection), "JsonCache\\" + subjectType);
            }
            catch (Exception e)
            {
                Debug.WriteLine("显示用户选定类别收藏信息失败。");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 显示时间表。
        /// </summary>
        /// <param name="bangumiCollection"></param>
        /// <returns></returns>
        public static async Task PopulateBangumiCalendarAsync(ObservableCollection<BangumiTimeLine> bangumiCollection, bool force = false)
        {
            try
            {
                //从文件反序列化
                var PreCalendar = JsonConvert.DeserializeObject<List<BangumiTimeLine>>(await FileHelper.ReadFromCacheFileAsync("JsonCache\\calendar"));
                bangumiCollection.Clear();
                int day = GetDayOfWeek();
                if (PreCalendar != null)
                {
                    foreach (var item in PreCalendar)
                    {
                        if (item.Weekday.Id <= day)
                        {
                            bangumiCollection.Add(item);
                        }
                        else
                        {
                            bangumiCollection.Insert(bangumiCollection.Count - day, item);
                        }
                    }
                    // 非强制加载，若缓存与当天为同一星期几则不请求新数据。
                    if (!force && bangumiCollection[0].Weekday.Id == GetDayOfWeek() + 1)
                    {
                        return;
                    }
                }

                var bangumiCalendarList = await BangumiHttpWrapper.GetBangumiCalendarAsync();

                //清空原数据
                bangumiCollection.Clear();
                foreach (var bangumiCalendar in bangumiCalendarList)
                {
                    if (bangumiCalendar.Weekday.Id <= day)
                    {
                        bangumiCollection.Add(bangumiCalendar);
                    }
                    else
                    {
                        bangumiCollection.Insert(bangumiCollection.Count - day, bangumiCalendar);
                    }
                }

                //将对象序列化并存储到文件
                await FileHelper.WriteToCacheFileAsync(JsonConvert.SerializeObject(bangumiCollection), "JsonCache\\calendar");
            }
            catch (Exception e)
            {
                Debug.WriteLine("显示时间表失败。");
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
            try
            {
                var result = await BangumiHttpWrapper.GetSubjectAsync(subjectId);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("获取指定条目详情失败。");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 获取指定条目收藏信息。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<SubjectStatus2> GetCollectionStatusAsync(string subjectId)
        {
            try
            {
                return await BangumiHttpWrapper.GetCollectionStatusAsync(OAuthHelper.MyToken.Token, subjectId);
            }
            catch (Exception e)
            {
                Debug.WriteLine("获取指定条目收藏信息失败。");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 获取用户指定条目收视进度。
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static async Task<Progress> GetProgressesAsync(string subjectId)
        {
            try
            {
                return await BangumiHttpWrapper.GetProgressesAsync(OAuthHelper.MyToken.UserId, OAuthHelper.MyToken.Token, subjectId);
            }
            catch (Exception e)
            {
                Debug.WriteLine("获取用户指定条目收视进度失败。");
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
        public static async Task<SearchResult> GetSearchResultAsync(string searchText, string type, int start, int n)
        {
            try
            {
                return await BangumiHttpWrapper.GetSearchResultAsync(searchText, type, start, n);
            }
            catch (Exception e)
            {
                Debug.WriteLine("获取搜索结果失败。");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }




        /// <summary>
        /// 更新收视进度。
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status"></param>
        /// <returns>更新是否成功</returns>
        public static async Task<bool> UpdateProgressAsync(string ep, EpStatusEnum status)
        {
            try
            {
                if (await BangumiHttpWrapper.UpdateProgressAsync(OAuthHelper.MyToken.Token, ep, status))
                {
                    MainPage.rootPage.ToastInAppNotification.Show($"标记章节{ep}{status.GetValue()}成功", 1500);
                    return true;
                }
                MainPage.rootPage.ErrorInAppNotification.Show($"标记章节{ep}{status.GetValue()}失败，请重试！", 1500);
                return false;
            }
            catch (Exception e)
            {
                MainPage.rootPage.ErrorInAppNotification.Show("更新收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
                //var msgDialog = new Windows.UI.Popups.MessageDialog("更新收视进度失败！\n" + e.Message) { Title = "错误！" };
                //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                //await msgDialog.ShowAsync();
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
            try
            {
                if (await BangumiHttpWrapper.UpdateProgressBatchAsync(OAuthHelper.MyToken.Token, ep, status, epsId))
                {
                    MainPage.rootPage.ToastInAppNotification.Show($"批量标记章节{epsId}{status.GetValue()}状态成功", 1500);
                    return true;
                }
                MainPage.rootPage.ErrorInAppNotification.Show($"批量标记章节{epsId}{status.GetValue()}状态失败，请重试！", 1500);
                return false;
            }
            catch (Exception e)
            {
                MainPage.rootPage.ErrorInAppNotification.Show("更新收藏进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
                //var msgDialog = new Windows.UI.Popups.MessageDialog("更新收藏状态失败！\n" + e.Message) { Title = "错误！" };
                //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                //await msgDialog.ShowAsync();
                return false;
            }
        }

        /// <summary>
        /// 更新收藏状态
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatusEnum"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privace"></param>
        /// <returns>更新是否成功</returns>
        public static async Task<bool> UpdateCollectionStatusAsync(string subjectId,
            CollectionStatusEnum collectionStatusEnum, string comment = "", string rating = "", string privace = "0")
        {
            try
            {
                if (await BangumiHttpWrapper.UpdateCollectionStatusAsync(OAuthHelper.MyToken.Token,
                    subjectId, collectionStatusEnum, comment, rating, privace))
                {
                    MainPage.rootPage.ToastInAppNotification.Show($"更新条目{subjectId}{collectionStatusEnum.GetValue()}状态成功", 1500);
                    return true;
                }
                MainPage.rootPage.ErrorInAppNotification.Show($"更新条目{subjectId}{collectionStatusEnum.GetValue()}状态失败，请重试！", 1500);
                return false;
            }
            catch (Exception e)
            {
                MainPage.rootPage.ErrorInAppNotification.Show("更新收藏进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
                //var msgDialog = new Windows.UI.Popups.MessageDialog("更新收藏状态失败！\n" + e.Message) { Title = "错误！" };
                //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                //await msgDialog.ShowAsync();
                return false;
            }
        }




        // 获取当天星期几
        public static int GetDayOfWeek()
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                case DayOfWeek.Sunday:
                    return 6;
                default:
                    return 0;
            }
        }

    }

}
