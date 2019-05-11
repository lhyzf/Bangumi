using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using System;
using System.Collections.ObjectModel;

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
                    Message = "网络连接失败，请重试！";
                }
            }
            else
            {
                Message = "请先登录！";
            }
            IsLoading = false;
        }


    }
}
