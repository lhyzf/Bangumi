using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using Bangumi.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Bangumi.ViewModels
{
    public class ProgressViewModel : ViewModelBase
    {
        public ProgressViewModel()
        {
            IsLoading = false;
        }

        public ObservableCollection<WatchingStatus> watchingCollection { get; private set; } = new ObservableCollection<WatchingStatus>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }


        /// <summary>
        /// 刷新收视进度列表。
        /// </summary>
        public async void LoadWatchingList()
        {
            if (OAuthHelper.IsLogin)
            {
                IsLoading = true;
                HomePage.homePage.isLoading = IsLoading;
                MainPage.rootPage.RefreshAppBarButton.IsEnabled = false;
                if (await BangumiFacade.PopulateWatchingListAsync(watchingCollection))
                {
                    Message = "更新时间：" + DateTime.Now;
                    CollectionSorting();
                    //将对象序列化并存储到文件
                    await FileHelper.WriteToCacheFile(JsonConvert.SerializeObject(watchingCollection), "JsonCache\\home");
                }
                else
                {
                    Message = "获取用户进度失败，请重试或重新登录！";
                }
            }
            else
            {
                Message = "请先登录！";
            }
            IsLoading = false;
            HomePage.homePage.isLoading = IsLoading;
            MainPage.rootPage.RefreshAppBarButton.IsEnabled = true;
        }

        // 更新下一章章节状态为已看
        public async void UpdateEpStatus(WatchingStatus item)
        {
            if (item != null)
            {
                if (item.next_ep != 0 && await BangumiFacade.UpdateProgressAsync(item.eps[item.next_ep - 1].id.ToString(), BangumiFacade.EpStatusEnum.watched))
                {
                    item.eps[item.next_ep - 1].status = "看过";
                    if (item.eps.Count == item.eps.Where(e => e.status == "看过").Count())
                        item.next_ep = 0;
                    else
                        item.next_ep++;
                    item.watched_eps = "看到第" + item.eps.Where(e => e.status == "看过").Count() + "话";
                    if (item.eps.Where(e => e.status == "看过").Count() < (item.eps.Count - item.eps.Where(e => e.status == "NA").Count()))
                        item.ep_color = "#d26585";
                    else
                        item.ep_color = "Gray";

                    //将对象序列化并存储到文件
                    await FileHelper.WriteToCacheFile(JsonConvert.SerializeObject(watchingCollection), "JsonCache\\home");

                }
            }
        }

        // 对条目进行排序
        private void CollectionSorting()
        {
            // 对条目进行排序
            var order = new List<WatchingStatus>();
            order.AddRange(watchingCollection.OrderBy(p => p.watched_eps).OrderBy(p => p.ep_color));
            for (int i = 0; i < order.Count; i++)
            {
                if (order[i].subject_id != watchingCollection[i].subject_id)
                {
                    for (int j = i + 1; j < order.Count; j++)
                    {
                        if (order[i].subject_id == watchingCollection[j].subject_id)
                        {
                            watchingCollection.Move(j, i);
                            //watchingCollection.RemoveAt(j);
                            //watchingCollection.Insert(i, order[i]);
                            break;
                        }
                    }
                }
            }
        }

    }
}
