using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.ViewModels
{
    public class TimeLineViewModel : ViewModelBase
    {
        public TimeLineViewModel()
        {
            IsLoading = false;
        }

        public ObservableCollection<BangumiTimeLine> bangumiCollection { get; private set; } = new ObservableCollection<BangumiTimeLine>();

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
        /// 刷新时间表。
        /// </summary>
        public async void LoadTimeLine()
        {
            IsLoading = true;
            if (await BangumiFacade.PopulateBangumiCalendarAsync(bangumiCollection))
            {
                Message = "更新时间：" + DateTime.Now;
            }
            else
            {
                Message = "网络连接失败，请重试！";
            }
            IsLoading = false;
        }


    }
}
