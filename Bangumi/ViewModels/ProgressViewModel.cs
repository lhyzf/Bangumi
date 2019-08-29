using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Api.Utils;
using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bangumi.ViewModels
{
    public class ProgressViewModel : ViewModelBase
    {
        public ProgressViewModel() => IsLoading = false;

        public ObservableCollection<WatchingStatus> WatchingCollection { get; private set; } = new ObservableCollection<WatchingStatus>();

        private object lockObj = new object();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }


        #region 公开方法

        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        /// <param name="status"></param>
        /// <param name="currentStatus"></param>
        public async void EditCollectionStatus(WatchingStatus status, CollectionStatusEnum currentStatus = CollectionStatusEnum.Do)
        {
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog()
            {
                Rate = 0,
                Comment = "",
                Privacy = false,
                CollectionStatus = currentStatus,
                SubjectType = (SubjectTypeEnum)status.Type
            };
            MainPage.RootPage.HasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync())
            {
                status.IsUpdating = true;
                if (await BangumiFacade.UpdateCollectionStatusAsync(status.SubjectId.ToString(),
                                                                    collectionEditContentDialog.CollectionStatus,
                                                                    collectionEditContentDialog.Comment,
                                                                    collectionEditContentDialog.Rate.ToString(),
                                                                    collectionEditContentDialog.Privacy == true ? "1" : "0"))
                {
                    // 若修改后状态不是在看，则从进度页面删除
                    if (collectionEditContentDialog.CollectionStatus != CollectionStatusEnum.Do)
                        WatchingCollection.Remove(status);
                }
                status.IsUpdating = false;
            }
            MainPage.RootPage.HasDialog = false;
        }

        /// <summary>
        /// 刷新收视进度列表。
        /// </summary>
        public async Task LoadWatchingListAsync()
        {
            try
            {
                if (BangumiApi.IsLogin)
                {
                    IsLoading = true;
                    HomePage.homePage.isLoading = IsLoading;
                    MainPage.RootPage.RefreshAppBarButton.IsEnabled = false;
                    await PopulateWatchingListAsync(WatchingCollection);
                }
                else
                {
                    //Message = "请先登录！";
                }
            }
            catch (Exception e)
            {
                if (e.Message.Equals("401"))
                {
                    // 授权过期，返回登录界面
                    MainPage.RootFrame.Navigate(typeof(LoginPage), "ms-appx:///Assets/resource/err_401.png");
                }
                else
                {
                    Debug.WriteLine("获取收视进度列表失败。");
                    Debug.WriteLine(e.Message);
                    NotificationHelper.Notify("获取收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                              NotificationHelper.NotifyType.Error);
                }
            }
            finally
            {
                IsLoading = false;
                HomePage.homePage.isLoading = IsLoading;
                MainPage.RootPage.RefreshAppBarButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 更新下一章章节状态为已看
        /// </summary>
        /// <param name="item"></param>
        public async void UpdateNextEpStatus(WatchingStatus item)
        {
            if (item != null && item.Eps != null && item.Eps.Count != 0)
            {
                item.IsUpdating = true;
                if (item.NextEp != -1 && await BangumiFacade.UpdateProgressAsync(
                    item.Eps.Where(ep => (ep.Status == "Air" || ep.Status == "Today" || ep.Status == "NA") && ep.Sort == item.NextEp)
                            .FirstOrDefault().Id.ToString(), EpStatusEnum.watched))
                {
                    item.Eps.Where(ep => (ep.Status == "Air" || ep.Status == "Today" || ep.Status == "NA") && ep.Sort == item.NextEp)
                            .FirstOrDefault().Status = "看过";
                    if (item.Eps.Count == item.Eps.Where(e => e.Status == "看过").Count())
                        item.NextEp = -1;
                    else
                        item.NextEp = item.Eps.Where(ep => ep.Status == "Air" || ep.Status == "Today").Count() != 0 ?
                                       item.Eps.Where(ep => ep.Status == "Air" || ep.Status == "Today").OrderBy(ep => ep.Sort).FirstOrDefault().Sort :
                                       item.Eps.Where(ep => ep.Status == "NA").FirstOrDefault().Sort;
                    item.WatchedEps++;
                    // 若未看到最新一集，则使用粉色，否则使用灰色
                    if (item.Eps.Where(e => e.Status == "看过").Count() < (item.Eps.Count - item.Eps.Where(e => e.Status == "NA").Count()))
                        item.EpColor = "#d26585";
                    else
                    {
                        // 将已看到最新剧集的条目排到最后，且设为灰色
                        if (WatchingCollection.IndexOf(item) != WatchingCollection.Count - 1)
                        {
                            WatchingCollection.Remove(item);
                            WatchingCollection.Add(item);
                        }
                        item.EpColor = "Gray";

                        // 若设置启用且看完则弹窗提示修改收藏状态及评价
                        if (SettingHelper.SubjectComplete && item.Eps.Where(e => e.Status == "看过").Count() == item.Eps.Count)
                        {
                            EditCollectionStatus(item, CollectionStatusEnum.Collect);
                        }
                    }
                    item.LastTouch = DateTime.Now.ConvertDateTimeToJsTick();

                }
                item.IsUpdating = false;
            }
        }

        #endregion

        #region 私有方法

        #region 进度
        /// <summary>
        /// 显示用户收视进度列表。
        /// </summary>
        /// <param name="watchingCollection"></param>
        /// <returns></returns>
        private async Task PopulateWatchingListAsync(ObservableCollection<WatchingStatus> watchingCollection)
        {
            try
            {
                List<Watching> watchingsCache = BangumiApi.BangumiCache.Watchings.ToList();
                // 加载缓存
                var cachedList = await ProcessWatchings(watchingsCache);
                if (!watchingCollection.SequenceEqualExT(cachedList))
                {
                    watchingCollection.Clear();
                    foreach (var item in cachedList)
                    {
                        watchingCollection.Add(item);
                    }
                }

                var watchings = await BangumiApi.GetWatchingListAsync();
                if (!SettingHelper.IsUpdatedToday)
                {
                    var newList = await ProcessWatchings(watchings, watchingCollection.ToList(), false);
                    if (!watchingCollection.SequenceEqualExT(newList))
                    {
                        watchingCollection.Clear();
                        foreach (var item in newList)
                        {
                            watchingCollection.Add(item);
                        }
                    }
                }
                else if (!watchingsCache.SequenceEqualExT(watchings))
                {
                    // 更新数据
                    var newList = await ProcessWatchings(watchings, watchingCollection.ToList(), false);
                    if (!watchingCollection.SequenceEqualExT(newList))
                    {
                        watchingCollection.Clear();
                        foreach (var item in newList)
                        {
                            watchingCollection.Add(item);
                        }
                    }
                }
                // 当天成功更新
                SettingHelper.IsUpdatedToday = true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 处理用户收视进度列表
        /// </summary>
        /// <param name="watchings">收视列表</param>
        /// <param name="fromCache">是否从缓存加载</param>
        /// <returns></returns>
        private async Task<List<WatchingStatus>> ProcessWatchings(List<Watching> watchings, List<WatchingStatus> cachedWatchings = null, bool fromCache = true)
        {
            List<WatchingStatus> watchList = new List<WatchingStatus>();
            //try
            //{
            //var deletedItems = new List<WatchingStatus>(); //标记要删除的条目
            //foreach (var sub in watchingCollection)
            //{
            //    //根据最新的进度删除原有条目
            //    if (watchings.Find(e => e.SubjectId == sub.SubjectId) == null)
            //    {
            //        deletedItems.Add(sub);
            //    }
            //}
            //foreach (var item in deletedItems) //删除条目
            //{
            //    watchingCollection.Remove(item);
            //}

            //var watchList = watchingCollection.ToList();

            var tasks = new List<Task>();
            // semaphore, allow to run 10 tasks in parallel
            using (var semaphore = new SemaphoreSlim(10))
            {
                foreach (var watching in watchings)
                {
                    await semaphore.WaitAsync();

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            ////若在看含有原来没有的条目则新增,之后再细化
                            //var item = watchList.Find(e => e.SubjectId == watching.SubjectId);
                            //if (item == null)
                            //{
                            var item = new WatchingStatus
                            {
                                Name = watching.Subject.Name,
                                NameCn = watching.Subject.NameCn,
                                Image = watching.Subject.Images.Common,
                                SubjectId = watching.SubjectId,
                                Url = watching.Subject.Url,
                                EpColor = "Gray",
                                LastTouch = watching.LastTouch, // 该条目上次修改时间
                                WatchedEps = watching.EpStatus,
                                UpdatedEps = watching.Subject.EpsCount,
                                AirDate = watching.Subject.AirDate,
                                AirWeekday = watching.Subject.AirWeekday,
                                Type = watching.Subject.Type,
                                IsUpdating = false,
                            };


                            // 加载缓存
                            if (fromCache)
                            {
                                //item.LastTouch = watching.LastTouch; // 该条目上次修改时间
                                if (BangumiApi.BangumiCache.Subjects.TryGetValue(item.SubjectId.ToString(), out Subject subjectCache))
                                {
                                    await ProcessSubject(item, subjectCache, fromCache);
                                }
                            }
                            else
                            {
                                // 将条目修改时间进行比较，仅更新有修改的条目，以及每天首次更新
                                if (item.LastTouch != cachedWatchings.Find(c => c.SubjectId == item.SubjectId)?.LastTouch ||
                                    !SettingHelper.IsUpdatedToday)
                                {
                                    var subject = await BangumiApi.GetSubjectAsync(item.SubjectId.ToString());
                                    // 更新数据
                                    await ProcessSubject(item, subject, fromCache);
                                }
                                else
                                {
                                    item = cachedWatchings.Find(c => c.SubjectId == item.SubjectId);
                                }
                                //item.LastUpdate = DateTime.Today.ConvertDateTimeToJsTick();
                            }
                            lock (lockObj)
                            {
                                // 集合的修改操作非线程安全，需加锁
                                watchList.Add(item);
                            }
                            //}
                            //else
                            //{
                            //    // 首次更新或条目有修改或 当天首次加载(当天未更新成功且条目未更新过)
                            //    if (item.LastTouch == 0 ||                                          // 条目首次更新
                            //        item.LastTouch != watching.LastTouch ||                         // 条目已修改
                            //        (!SettingHelper.IsUpdatedToday &&                               // 今天未更新成功
                            //        item.LastUpdate != DateTime.Today.ConvertDateTimeToJsTick()))   // 条目今天未更新
                            //    {
                            //        // 加载缓存
                            //        BangumiApi.BangumiCache.Subjects.TryGetValue(item.SubjectId.ToString(), out Subject subjectCache);
                            //        await ProcessSubject(item, subjectCache, fromCache);

                            //        if (!fromCache)
                            //        {
                            //            var subject = await BangumiApi.GetSubjectAsync(item.SubjectId.ToString());
                            //            if (!subjectCache.EqualsExT(subject))
                            //            {
                            //                // 更新数据
                            //                await ProcessSubject(item, subject, fromCache);
                            //            }
                            //        }
                            //    }
                            //}
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));

                }
                // await for the rest of tasks to complete
                await Task.WhenAll(tasks);
                return CollectionSorting(watchList);
                //watchingCollection.Clear();
                //foreach (var item in CollectionSorting(watchList))
                //{
                //    watchingCollection.Add(item);
                //}
            }

            //}
            //catch (Exception)
            //{

            //    throw;
            //}
        }

        /// <summary>
        /// 处理条目章节
        /// </summary>
        /// <param name="item">显示条目</param>
        /// <param name="subject">条目章节</param>
        /// <param name="fromCache">是否从缓存加载</param>
        /// <returns></returns>
        private async Task ProcessSubject(WatchingStatus item, Subject subject, bool fromCache)
        {
            //item.IsUpdating = true;

            item.Eps = new List<SimpleEp>();
            if (subject?.Eps != null)
            {
                foreach (var ep in subject.Eps)
                {
                    SimpleEp simpleEp = new SimpleEp
                    {
                        Id = ep.Id,
                        Sort = ep.Sort,
                        Status = ep.Status,
                        Type = ep.Type,
                        Name = ep.NameCn == "" ? ep.Name : ep.NameCn
                    };
                    item.Eps.Add(simpleEp);
                }
                item.UpdatedEps = item.Eps.Count - item.Eps.Where(e => e.Status == "NA").Count();

                if (fromCache)
                {
                    if (BangumiApi.BangumiCache.Progresses.TryGetValue(item.SubjectId.ToString(), out Progress progressCache))
                    {
                        // 加载缓存
                        ProcessProgress(item, progressCache);
                    }
                }
                else
                {
                    var progress = await BangumiApi.GetProgressesAsync(item.SubjectId.ToString());
                    // 更新数据
                    ProcessProgress(item, progress);
                }

            }
            else
            {
                item.WatchedEps = -1;
                item.UpdatedEps = -1;
                item.EpColor = "Gray";
            }

            if (item.NextEp != -1 && item.Eps.Count != 0)
            {
                item.NextEp = item.Eps.Where(ep => ep.Status == "Air" || ep.Status == "Today").Count() != 0 ?
                              item.Eps.Where(ep => ep.Status == "Air" || ep.Status == "Today").OrderBy(ep => ep.Sort).FirstOrDefault().Sort :
                              item.Eps.Where(ep => ep.Status == "NA").FirstOrDefault().Sort;
            }
        }

        /// <summary>
        /// 处理用户进度
        /// </summary>
        /// <param name="item">显示条目</param>
        /// <param name="progress">进度</param>
        private void ProcessProgress(WatchingStatus item, Progress progress)
        {
            if (progress != null)
            {
                item.WatchedEps = progress.Eps.Count;
                if (item.Eps.Count == item.WatchedEps)
                {
                    item.NextEp = -1;
                }
                if (progress.Eps.Count < (item.Eps.Count - item.Eps.Where(e => e.Status == "NA").Count()))
                {
                    item.EpColor = "#d26585";
                }
                else
                {
                    item.EpColor = "Gray";
                }
                foreach (var ep in item.Eps) // 填充用户观看状态
                {
                    var temp = progress.Eps.ToList();
                    foreach (var p in temp)
                    {
                        if (p.Id == ep.Id)
                        {
                            ep.Status = p.Status.CnName;
                            temp.Remove(p);
                            break;
                        }
                    }
                }
            }
            else
            {
                item.WatchedEps = 0;
                if (item.UpdatedEps != 0)
                {
                    item.EpColor = "#d26585";
                }
            }
        }

        #endregion

        /// <summary>
        /// 对条目进行排序
        /// </summary>
        private List<WatchingStatus> CollectionSorting(List<WatchingStatus> watchingStatuses)
        {
            var order = new List<WatchingStatus>();
            var notWatched = new List<WatchingStatus>();
            var allWatched = new List<WatchingStatus>();
            order = watchingStatuses
                .Where(p => p.WatchedEps != 0 && p.WatchedEps != p.Eps.Count)
                .OrderBy(p => p.LastTouch)
                .OrderBy(p => p.EpColor)
                .ToList();
            notWatched = watchingStatuses
                .Where(p => p.WatchedEps == 0 && p.WatchedEps != p.Eps.Count)
                .OrderBy(p => p.EpColor)
                .ToList();
            allWatched = watchingStatuses
                .Where(p => p.WatchedEps == p.Eps.Count)
                .ToList();

            // 排序，尚未观看的排在所有有观看记录的有更新的条目之后，
            // ，在已看到最新剧集的条目之前，看完的排在最后
            for (int i = 0; i <= order.Count; i++)
            {
                if (i == order.Count || order[i].EpColor == "Gray")
                {
                    order.InsertRange(i, notWatched);
                    break;
                }
            }
            order.AddRange(allWatched);

            // 仅修改与排序不同之处
            for (int i = 0; i < watchingStatuses.Count; i++)
            {
                if (watchingStatuses[i].SubjectId != order[i].SubjectId)
                {
                    watchingStatuses.RemoveAt(i);
                    watchingStatuses.Insert(i, order[i]);
                }
            }
            return watchingStatuses;
        }

        #endregion

    }



    public class WatchingStatus : ViewModelBase
    {
        public string Name { get; set; }
        public string NameCn { get; set; }
        public int SubjectId { get; set; }
        public long LastTouch { get; set; }
        public long LastUpdate { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string AirDate { get; set; }
        public int Type { get; set; }
        public int AirWeekday { get; set; }
        public List<SimpleEp> Eps { get; set; }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set => Set(ref _isUpdating, value);
        }

        private int _watched_eps;
        public int WatchedEps
        {
            get { return _watched_eps; }
            set => Set(ref _watched_eps, value);
        }

        private int _updated_eps;
        public int UpdatedEps
        {
            get { return _updated_eps; }
            set => Set(ref _updated_eps, value);
        }

        private double _next_ep;
        public double NextEp
        {
            get => _next_ep;
            set => Set(ref _next_ep, value);
        }

        private string _ep_color;
        public string EpColor
        {
            get { return _ep_color; }
            set => Set(ref _ep_color, value);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            WatchingStatus w = (WatchingStatus)obj;
            return SubjectId == w.SubjectId &&
                   LastTouch == w.LastTouch &&
                   LastUpdate == w.LastUpdate &&
                   Type == w.Type &&
                   AirWeekday == w.AirWeekday &&
                   IsUpdating == w.IsUpdating &&
                   WatchedEps == w.WatchedEps &&
                   UpdatedEps == w.UpdatedEps &&
                   NextEp == w.NextEp &&
                   EpColor.EqualsExT(w.EpColor) &&
                   Name.EqualsExT(w.Name) &&
                   NameCn.EqualsExT(w.NameCn) &&
                   Url.EqualsExT(w.Url) &&
                   Image.EqualsExT(w.Image) &&
                   AirDate.EqualsExT(w.AirDate) &&
                   Eps.SequenceEqualExT(w.Eps);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return SubjectId;
        }
    }

    public class SimpleEp
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public double Sort { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SimpleEp s = (SimpleEp)obj;
            return Id == s.Id &&
                   Type == s.Type &&
                   Sort == s.Sort &&
                   Status.EqualsExT(s.Status) &&
                   Name.EqualsExT(s.Name);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
