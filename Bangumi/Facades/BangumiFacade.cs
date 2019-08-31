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
                List<BangumiTimeLine> cache = BangumiApi.BangumiCache.TimeLine.ToList();
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
                if (!force && bangumiTimeLine.Count > 0 && bangumiTimeLine[0].Weekday.Id == day)
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
                    NotificationHelper.Notify($"标记章节{ep}{status.GetCnName()}成功");
                    return true;
                }
                NotificationHelper.Notify($"标记章节{ep}{status.GetCnName()}失败，请重试！",
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
                    NotificationHelper.Notify($"批量标记章节{epsId}{status.GetCnName()}状态成功");
                    return true;
                }
                NotificationHelper.Notify($"批量标记章节{epsId}{status.GetCnName()}状态失败，请重试！",
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
