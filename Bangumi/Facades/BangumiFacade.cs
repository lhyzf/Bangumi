﻿using Bangumi.Api.Models;
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
                if (watchingListCollection.Count == 0)
                {
                    var preWatchings = JsonConvert.DeserializeObject<List<WatchingStatus>>(await FileHelper.ReadFromCacheFileAsync(OAuthHelper.CacheFile.Progress.GetFilePath()));
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

                var watchingList = await BangumiHttpWrapper.GetWatchingListAsync(OAuthHelper.MyToken.UserId);

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
                            var subject = await BangumiHttpWrapper.GetSubjectEpsAsync(item.SubjectId.ToString());

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

                                var progress = await GetProgressesAsync(item.SubjectId.ToString());
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
                var preCollection = JsonConvert.DeserializeObject<List<Collection>>(await FileHelper.ReadFromCacheFileAsync(subjectType.GetFilePath()));
                subjectCollection.Clear();
                if (preCollection != null)
                {
                    foreach (var type in preCollection)
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
                await FileHelper.WriteToCacheFileAsync(JsonConvert.SerializeObject(subjectCollection), subjectType.GetFilePath());
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
                var preCalendar = JsonConvert.DeserializeObject<List<BangumiTimeLine>>(await FileHelper.ReadFromCacheFileAsync(OAuthHelper.CacheFile.Calendar.GetFilePath()));
                bangumiCollection.Clear();
                int day = GetDayOfWeek();
                if (preCalendar != null)
                {
                    foreach (var item in preCalendar)
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
                await FileHelper.WriteToCacheFileAsync(JsonConvert.SerializeObject(bangumiCollection.OrderBy(c => c.Weekday.Id)), OAuthHelper.CacheFile.Calendar.GetFilePath());
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
                    MainPage.RootPage.ToastInAppNotification.Show($"标记章节{ep}{status.GetValue()}成功", 1500);
                    return true;
                }
                MainPage.RootPage.ErrorInAppNotification.Show($"标记章节{ep}{status.GetValue()}失败，请重试！", 1500);
                return false;
            }
            catch (Exception e)
            {
                MainPage.RootPage.ErrorInAppNotification.Show("更新收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
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
                    MainPage.RootPage.ToastInAppNotification.Show($"批量标记章节{epsId}{status.GetValue()}状态成功", 1500);
                    return true;
                }
                MainPage.RootPage.ErrorInAppNotification.Show($"批量标记章节{epsId}{status.GetValue()}状态失败，请重试！", 1500);
                return false;
            }
            catch (Exception e)
            {
                MainPage.RootPage.ErrorInAppNotification.Show("更新收藏进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
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
                if (await BangumiHttpWrapper.UpdateCollectionStatusAsync(OAuthHelper.MyToken.Token,
                    subjectId, collectionStatus, comment, rating, privace))
                {
                    MainPage.RootPage.ToastInAppNotification.Show($"更新条目{subjectId}状态成功", 1500);
                    return true;
                }
                MainPage.RootPage.ErrorInAppNotification.Show($"更新条目{subjectId}状态失败，请重试！", 1500);
                return false;
            }
            catch (Exception e)
            {
                MainPage.RootPage.ErrorInAppNotification.Show("更新收藏进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
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
