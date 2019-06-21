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
        public async void EditCollectionStatus(WatchingStatus status, string currentStatus = "在看")
        {
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog()
            {
                rate = 0,
                comment = "",
                privacy = false,
                collectionStatus = currentStatus
            };
            MainPage.rootPage.hasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync())
            {
                status.isUpdating = true;
                if (await BangumiFacade.UpdateCollectionStatusAsync(status.subject_id.ToString(),
                                                                    BangumiConverters.GetCollectionStatusEnum(collectionEditContentDialog.collectionStatus),
                                                                    collectionEditContentDialog.comment,
                                                                    collectionEditContentDialog.rate.ToString(),
                                                                    collectionEditContentDialog.privacy == true ? "1" : "0"))
                {
                    if (collectionEditContentDialog.collectionStatus != "在看")
                        await LoadWatchingListAsync();
                    if (collectionEditContentDialog.collectionStatus == CollectionStatusEnum.collect.GetValue() && SettingHelper.EpsBatch == true)
                    {
                        int epId = 0;
                        string epsId = string.Empty;
                        foreach (var episode in status.eps)
                        {
                            if (status.eps.IndexOf(episode) == status.eps.Count - 1)
                            {
                                epsId += episode.id.ToString();
                                epId = episode.id;
                                break;
                            }
                            else
                            {
                                epsId += episode.id.ToString() + ",";
                            }
                        }
                        if (await BangumiFacade.UpdateProgressBatchAsync(epId, EpStatusEnum.watched, epsId))
                        {
                            foreach (var episode in status.eps)
                            {
                                episode.status = "看过";
                            }
                        }
                    }
                }
                status.isUpdating = false;
            }
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
                    MainPage.rootPage.RefreshAppBarButton.IsEnabled = false;
                    await BangumiFacade.PopulateWatchingListAsync(WatchingCollection);
                    CollectionSorting();
                    //将对象序列化并存储到文件
                    await FileHelper.WriteToCacheFileAsync(JsonConvert.SerializeObject(WatchingCollection), "JsonCache\\home");
                }
                else
                {
                    //Message = "请先登录！";
                }
            }
            catch (Exception e)
            {
                MainPage.rootPage.ErrorInAppNotification.Show("获取收视进度失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
                //var msgDialog = new Windows.UI.Popups.MessageDialog("获取收视进度失败！\n" + e.Message) { Title = "错误！" };
                //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                //await msgDialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
                HomePage.homePage.isLoading = IsLoading;
                MainPage.rootPage.RefreshAppBarButton.IsEnabled = true;
            }
        }

        // 更新下一章章节状态为已看
        public async void UpdateNextEpStatus(WatchingStatus item)
        {
            if (item != null)
            {
                item.isUpdating = true;
                if (item.next_ep != 0 && await BangumiFacade.UpdateProgressAsync(
                    item.eps.Where(ep => ep.sort == item.next_ep).FirstOrDefault().id.ToString(), EpStatusEnum.watched))
                {
                    item.eps.Where(ep => ep.sort == item.next_ep).FirstOrDefault().status = "看过";
                    if (item.eps.Count == item.eps.Where(e => e.status == "看过").Count())
                        item.next_ep = 0;
                    else
                        item.next_ep = item.eps.Where(ep => ep.status == "Air" || ep.status == "Today" || ep.status == "NA").FirstOrDefault().sort;
                    item.watched_eps++;
                    // 若未看到最新一集，则使用粉色，否则使用灰色
                    if (item.eps.Where(e => e.status == "看过").Count() < (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()))
                        item.ep_color = "#d26585";
                    else
                    {
                        // 将已看到最新剧集的条目排到最后，且设为灰色
                        if (WatchingCollection.IndexOf(item) != WatchingCollection.Count - 1)
                        {
                            WatchingCollection.Remove(item);
                            WatchingCollection.Add(item);
                        }
                        item.ep_color = "Gray";

                        // 若设置启用且看完则弹窗提示修改收藏状态及评价
                        if (SettingHelper.SubjectComplete == true && item.eps.Where(e => e.status == "看过").Count() == item.eps.Count)
                        {
                            EditCollectionStatus(item, "看过");
                        }
                    }
                    item.lasttouch = DateTime.Now.ConvertDateTimeToJsTick();

                    //将对象序列化并存储到文件
                    await FileHelper.WriteToCacheFileAsync(JsonConvert.SerializeObject(WatchingCollection), "JsonCache\\home");

                }
                item.isUpdating = false;
            }
        }

        // 对条目进行排序
        private void CollectionSorting()
        {
            var order = new List<WatchingStatus>();
            var notWatched = new List<WatchingStatus>();
            var allWatched = new List<WatchingStatus>();
            order = WatchingCollection
                .Where(p => p.watched_eps != 0 && p.watched_eps != p.eps.Count)
                .OrderBy(p => p.lasttouch)
                .OrderBy(p => p.ep_color)
                .ToList();
            notWatched = WatchingCollection
                .Where(p => p.watched_eps == 0)
                .ToList();
            allWatched = WatchingCollection
                .Where(p => p.watched_eps == p.eps.Count)
                .ToList();

            // 排序，尚未观看的排在所有有观看记录的有更新的条目之后，
            // ，在已看到最新剧集的条目之前，看完的排在最后
            for (int i = 0; i < order.Count; i++)
            {
                if (order[i].ep_color == "Gray")
                {
                    order.InsertRange(i, notWatched);
                    break;
                }
            }
            order.AddRange(allWatched);

            // 仅修改与排序不同之处
            for (int i = 0; i < WatchingCollection.Count; i++)
            {
                if (WatchingCollection[i].subject_id != order[i].subject_id)
                {
                    WatchingCollection.RemoveAt(i);
                    WatchingCollection.Insert(i, order[i]);
                }
            }
        }

    }

    public class WatchingStatus : ViewModelBase
    {
        public string name { get; set; }
        public string name_cn { get; set; }
        public int subject_id { get; set; }
        public long lasttouch { get; set; }
        public long lastupdate { get; set; }
        public string url { get; set; }
        public string image { get; set; }
        public List<SimpleEp> eps { get; set; }

        private bool _isUpdating;
        public bool isUpdating
        {
            get => _isUpdating;
            set => Set(ref _isUpdating, value);
        }

        private int _watched_eps;
        public int watched_eps
        {
            get { return _watched_eps; }
            set => Set(ref _watched_eps, value);
        }

        private string _updated_eps;
        public string updated_eps
        {
            get { return _updated_eps; }
            set => Set(ref _updated_eps, value);
        }

        private float _next_ep;
        public float next_ep
        {
            get => _next_ep;
            set => Set(ref _next_ep, value);
        }

        private string _ep_color;
        public string ep_color
        {
            get { return _ep_color; }
            set => Set(ref _ep_color, value);
        }
    }

    public class SimpleEp
    {
        public int id { get; set; }
        public int type { get; set; }
        public float sort { get; set; }
        public string status { get; set; }
        public string name { get; set; }
    }
}
