using Bangumi.Api.Models;
using Bangumi.Api.Utils;
using Bangumi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bangumi.Api;
using Bangumi.Helper;

namespace Bangumi.Facades
{
    public static class BangumiFacade
    {
        #region 首页 进度、收藏、时间表 列表显示

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
                if (watchingListCollection.Count == 0)
                {
                    var preWatchings = JsonConvert.DeserializeObject<List<WatchingStatus>>(await Helper.FileHelper.ReadFromCacheFileAsync(OAuthHelper.CacheFile.Progress.GetFilePath()));
                    if (preWatchings != null)
                    {
                        foreach (var sub in preWatchings)
                        {
                            // 将Collection中没有的添加进去
                            if (watchingListCollection.Where(e => e.SubjectId == sub.SubjectId).Count() == 0)
                                watchingListCollection.Add(sub);
                        }
                    }
                }

                var watchingList = await BangumiApi.GetWatchingListAsync();

                var deletedItems = new List<WatchingStatus>(); //标记要删除的条目
                foreach (var sub in watchingListCollection)
                {
                    //根据最新的进度删除原有条目
                    if (watchingList.Where(e => e.SubjectId == sub.SubjectId).Count() == 0)
                        deletedItems.Add(sub);
                }
                foreach (var item in deletedItems) //删除条目
                {
                    watchingListCollection.Remove(item);
                }

                foreach (var watching in watchingList)
                {
                    //若在看含有原来没有的条目则新增,之后再细化
                    var item = watchingListCollection.Where(e => e.SubjectId == watching.SubjectId).FirstOrDefault();
                    if (item == null)
                    {
                        WatchingStatus watchingStatus = new WatchingStatus();
                        watchingStatus.Name = watching.Subject.Name;
                        watchingStatus.NameCn = watching.Subject.NameCn;
                        watchingStatus.Image = watching.Subject.Images.Common;
                        watchingStatus.SubjectId = watching.SubjectId;
                        watchingStatus.Url = watching.Subject.Url;
                        watchingStatus.EpColor = "Gray";
                        watchingStatus.LastTouch = 0;
                        watchingStatus.WatchedEps = watching.EpStatus;
                        watchingStatus.UpdatedEps = watching.Subject.EpsCount;
                        watchingStatus.AirDate = watching.Subject.AirDate;
                        watchingStatus.AirWeekday = watching.Subject.AirWeekday;
                        watchingStatus.Type = watching.Subject.Type;

                        watchingListCollection.Add(watchingStatus);
                    }
                }
                foreach (var watching in watchingList)
                {
                    var item = watchingListCollection.Where(e => e.SubjectId == watching.SubjectId).FirstOrDefault();
                    if (item != null)
                    {
                        item.IsUpdating = true;
                        // 首次更新或条目有修改或当天首次加载
                        if (item.LastTouch == 0 ||
                            item.LastTouch != watching.LastTouch ||
                            item.LastUpdate != DateTime.Today.ConvertDateTimeToJsTick())
                        {
                            // 获取EP信息
                            var subject = await BangumiApi.GetSubjectEpsAsync(item.SubjectId.ToString());

                            item.Eps = new List<SimpleEp>();
                            if (subject.Eps != null)
                            {
                                foreach (var ep in subject.Eps)
                                {
                                    SimpleEp simpleEp = new SimpleEp();
                                    simpleEp.Id = ep.Id;
                                    simpleEp.Sort = ep.Sort;
                                    simpleEp.Status = ep.Status;
                                    simpleEp.Type = ep.Type;
                                    simpleEp.Name = ep.NameCn == "" ? ep.Name : ep.NameCn;
                                    item.Eps.Add(simpleEp);
                                }
                                item.UpdatedEps = item.Eps.Count - item.Eps.Where(e => e.Status == "NA").Count();

                                var progress = await BangumiApi.GetProgressesAsync(item.SubjectId.ToString());
                                if (progress != null)
                                {
                                    item.WatchedEps = progress.Eps.Count;
                                    if (item.Eps.Count == item.WatchedEps)
                                        item.NextEp = -1;
                                    if (progress.Eps.Count < (item.Eps.Count - item.Eps.Where(e => e.Status == "NA").Count()))
                                        item.EpColor = "#d26585";
                                    else
                                        item.EpColor = "Gray";
                                    foreach (var ep in item.Eps) // 填充用户观看状态
                                    {
                                        foreach (var p in progress.Eps)
                                        {
                                            if (p.Id == ep.Id)
                                            {
                                                ep.Status = p.Status.CnName;
                                                progress.Eps.Remove(p);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    item.WatchedEps = 0;
                                    if (item.UpdatedEps != 0)
                                        item.EpColor = "#d26585";
                                }
                            }
                            else
                            {
                                item.WatchedEps = -1;
                                item.UpdatedEps = -1;
                                item.EpColor = "Gray";
                            }

                            if (item.NextEp != -1 && item.Eps.Count != 0)
                                item.NextEp = item.Eps.Where(ep => ep.Status == "Air" || ep.Status == "Today").Count() != 0 ?
                                               item.Eps.Where(ep => ep.Status == "Air" || ep.Status == "Today").OrderBy(ep => ep.Sort).FirstOrDefault().Sort :
                                               item.Eps.Where(ep => ep.Status == "NA").FirstOrDefault().Sort;
                            item.LastTouch = watching.LastTouch;
                            item.LastUpdate = DateTime.Today.ConvertDateTimeToJsTick();
                        }
                        item.IsUpdating = false;
                    }
                }
            }
            catch (Exception e)
            {
                foreach (var item in watchingListCollection.Where(i => i.IsUpdating == true))
                {
                    item.IsUpdating = false;
                }
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
                // 获取缓存
                BangumiApi.BangumiCache.Collections.TryGetValue(subjectType.GetValue(), out Collection2 cache);
                if (cache != null && !cache.Collects.SequenceEqualExT(subjectCollection.ToList()))
                {
                    //清空原数据
                    subjectCollection.Clear();
                    foreach (var status in cache.Collects)
                    {
                        subjectCollection.Add(status);
                    }
                }

                var respose = await BangumiApi.GetSubjectCollectionAsync(subjectType);

                if (!cache.EqualsExT(respose))
                {
                    //清空原数据
                    subjectCollection.Clear();
                    foreach (var status in respose.Collects)
                    {
                        subjectCollection.Add(status);
                    }
                }
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
        /// <param name="bangumiTimeLine"></param>
        /// <returns></returns>
        public static async Task PopulateBangumiCalendarAsync(ObservableCollection<BangumiTimeLine> bangumiTimeLine, bool force = false)
        {
            try
            {
                List<BangumiTimeLine> cache = BangumiApi.BangumiCache.TimeLine;
                int day = GetDayOfWeek();
                if (!cache.SequenceEqualExT(bangumiTimeLine.OrderBy(b => b.Weekday.Id).ToList()))
                {
                    bangumiTimeLine.Clear();
                    foreach (var item in cache)
                    {
                        if (item.Weekday.Id < day)
                        {
                            bangumiTimeLine.Add(item);
                        }
                        else
                        {
                            bangumiTimeLine.Insert(bangumiTimeLine.Count + 1 - day, item);
                        }
                    }
                }

                // 非强制加载，若缓存与当天为同一星期几则不请求新数据。
                if (!force && bangumiTimeLine[0].Weekday.Id == day)
                {
                    return;
                }

                var response = await BangumiApi.GetBangumiCalendarAsync();

                if (!cache.SequenceEqualExT(response))
                {
                    //清空原数据
                    bangumiTimeLine.Clear();
                    foreach (var item in response)
                    {
                        if (item.Weekday.Id < day)
                        {
                            bangumiTimeLine.Add(item);
                        }
                        else
                        {
                            bangumiTimeLine.Insert(bangumiTimeLine.Count + 1 - day, item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("显示时间表失败。");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        #endregion


        #region 更新进度、状态，并显示通知

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
                if (await BangumiApi.UpdateProgressAsync(ep, status))
                {
                    NotificationHelper.Notify($"标记章节{ep}{status.GetValue()}成功");
                    return true;
                }
                NotificationHelper.Notify($"标记章节{ep}{status.GetValue()}失败，请重试！",
                                          NotificationHelper.NotifyType.Warn);
                return false;
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("更新收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
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
                if (await BangumiApi.UpdateProgressBatchAsync(ep, status, epsId))
                {
                    NotificationHelper.Notify($"批量标记章节{epsId}{status.GetValue()}状态成功");
                    return true;
                }
                    NotificationHelper.Notify($"批量标记章节{epsId}{status.GetValue()}状态失败，请重试！",
                                              NotificationHelper.NotifyType.Warn);
                return false;
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("批量标记章节状态失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
                return false;
            }
        }

        /// <summary>
        /// 更新收藏状态
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="collectionStatus"></param>
        /// <param name="comment"></param>
        /// <param name="rating"></param>
        /// <param name="privace"></param>
        /// <returns>更新是否成功</returns>
        public static async Task<bool> UpdateCollectionStatusAsync(string subjectId,
            CollectionStatusEnum collectionStatus, string comment = "", string rating = "", string privace = "0")
        {
            try
            {
                if (await BangumiApi.UpdateCollectionStatusAsync(subjectId, collectionStatus, comment, rating, privace))
                {
                    NotificationHelper.Notify($"更新条目{subjectId}状态成功");
                    return true;
                }
                NotificationHelper.Notify($"更新条目{subjectId}状态失败，请重试！",
                                          NotificationHelper.NotifyType.Warn);
                return false;
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("更新条目状态失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
                return false;
            }
        }

        #endregion


        // 获取当天星期几
        public static int GetDayOfWeek()
        {
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                case DayOfWeek.Sunday:
                    return 7;
                default:
                    return 1;
            }
        }

    }

}
