using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Api.Utils;
using Bangumi.Api.Models;
using Bangumi.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bangumi.ViewModels
{
    public class ProgressViewModel : ViewModelBase
    {
        public ProgressViewModel() => IsLoading = false;

        public ObservableCollection<WatchingStatus> WatchingCollection { get; private set; } = new ObservableCollection<WatchingStatus>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        // 更新收藏状态、评分、吐槽
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
                if (OAuthHelper.IsLogin)
                {
                    IsLoading = true;
                    HomePage.homePage.isLoading = IsLoading;
                    MainPage.RootPage.RefreshAppBarButton.IsEnabled = false;
                    await BangumiFacade.PopulateWatchingListAsync(WatchingCollection);
                    CollectionSorting();
                    //将对象序列化并存储到文件
                    await FileHelper.WriteToCacheFileAsync(JsonConvert.SerializeObject(WatchingCollection), OAuthHelper.CacheFile.Progress.GetFilePath());
                }
                else
                {
                    //Message = "请先登录！";
                }
            }
            catch (Exception e)
            {
                MainPage.RootPage.ErrorInAppNotification.Show("获取收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
                //var msgDialog = new Windows.UI.Popups.MessageDialog("获取收视进度失败！\n" + e.Message) { Title = "错误！" };
                //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                //await msgDialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
                HomePage.homePage.isLoading = IsLoading;
                MainPage.RootPage.RefreshAppBarButton.IsEnabled = true;
            }
        }

        // 更新下一章章节状态为已看
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

                    //将对象序列化并存储到文件
                    await FileHelper.WriteToCacheFileAsync(JsonConvert.SerializeObject(WatchingCollection), OAuthHelper.CacheFile.Progress.GetFilePath());

                }
                item.IsUpdating = false;
            }
        }

        // 对条目进行排序
        private void CollectionSorting()
        {
            var order = new List<WatchingStatus>();
            var notWatched = new List<WatchingStatus>();
            var allWatched = new List<WatchingStatus>();
            order = WatchingCollection
                .Where(p => p.WatchedEps != 0 && p.WatchedEps != p.Eps.Count)
                .OrderBy(p => p.LastTouch)
                .OrderBy(p => p.EpColor)
                .ToList();
            notWatched = WatchingCollection
                .Where(p => p.WatchedEps == 0 && p.WatchedEps != p.Eps.Count)
                .OrderBy(p => p.EpColor)
                .ToList();
            allWatched = WatchingCollection
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
            for (int i = 0; i < WatchingCollection.Count; i++)
            {
                if (WatchingCollection[i].SubjectId != order[i].SubjectId)
                {
                    WatchingCollection.RemoveAt(i);
                    WatchingCollection.Insert(i, order[i]);
                }
            }
        }

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
    }

    public class SimpleEp
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public double Sort { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
    }
}
