using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bangumi.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public HomeViewModel()
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
                if (await BangumiFacade.PopulateWatchingListAsync(watchingCollection))
                {
                    Message = "更新时间：" + DateTime.Now;
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
        }

        public async void UpdateEpStatus(WatchingStatus item)
        {
            if (item != null)
            {
                IsLoading = true;
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
                }
                IsLoading = false;
            }
        }


    }
}
