using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
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
        public async void EditCollectionStatus(WatchingStatus status)
        {
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog()
            {
                rate = 0,
                comment = "",
                privacy = false,
                collectionStatus = "在看"
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
                var msgDialog = new Windows.UI.Popups.MessageDialog("获取收视进度失败！\n" + e.Message) { Title = "错误！" };
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                await msgDialog.ShowAsync();
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
                if (item.next_ep != 0 && await BangumiFacade.UpdateProgressAsync(item.eps[item.next_ep - 1].id.ToString(), EpStatusEnum.watched))
                {
                    item.eps[item.next_ep - 1].status = "看过";
                    if (item.eps.Count == item.eps.Where(e => e.status == "看过").Count())
                        item.next_ep = 0;
                    else
                        item.next_ep++;
                    item.watched_eps = item.eps.Where(e => e.status == "看过").Count();
                    // 若未看到最新一集，则使用粉色，否则使用灰色
                    if (item.eps.Where(e => e.status == "看过").Count() < (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()))
                        item.ep_color = "#d26585";
                    else
                    {
                        // 若设置启用且看完则标记为看过，并从列表中删除
                        if (SettingHelper.SubjectComplete == true && item.eps.Where(e => e.status == "看过").Count() == item.eps.Count)
                        {
                            await BangumiFacade.UpdateCollectionStatusAsync(item.subject_id.ToString(), CollectionStatusEnum.collect);
                            WatchingCollection.Remove(item);
                        }
                        else
                        {
                            // 将已看到最新剧集的条目排到最后
                            WatchingCollection.Remove(item);
                            WatchingCollection.Add(item);
                            item.ep_color = "Gray";
                        }
                    }

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
            order.AddRange(WatchingCollection.OrderBy(p => p.lasttouch).OrderBy(p => p.ep_color));
            for (int i = 0; i < order.Count; i++)
            {
                if (order[i].watched_eps == 0)
                {
                    notWatched.Add(order[i]);
                    order.Remove(order[i]);
                    i--;
                }
            }
            WatchingCollection.Clear();
            foreach (var item in order)
            {
                if (notWatched.Count != 0 && item.ep_color == "Gray")
                {
                    foreach (var item2 in notWatched)
                    {
                        WatchingCollection.Add(item2);
                    }
                    notWatched.Clear();
                }
                WatchingCollection.Add(item);
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

        private int _watched_eps;
        private string _updated_eps;
        private string _ep_color;
        private int _next_ep;
        private bool _isUpdating;

        public int next_ep
        {
            get => _next_ep;
            set => Set(ref _next_ep, value);
        }
        public bool isUpdating
        {
            get => _isUpdating;
            set => Set(ref _isUpdating, value);
        }
        public string ep_color
        {
            get { return _ep_color; }
            set => Set(ref _ep_color, value);
        }
        public int watched_eps
        {
            get { return _watched_eps; }
            set => Set(ref _watched_eps, value);
        }
        public string updated_eps
        {
            get { return _updated_eps; }
            set => Set(ref _updated_eps, value);
        }
    }

    public class SimpleEp
    {
        public int id { get; set; }
        public int type { get; set; }
        public string sort { get; set; }
        public string status { get; set; }
        public string name { get; set; }
    }
}
