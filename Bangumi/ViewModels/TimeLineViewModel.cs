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

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value);
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
                SelectedIndex = GetDayOfWeek();
            }
            else
            {
                Message = "网络连接失败，请重试！";
            }
            IsLoading = false;
        }

        private int GetDayOfWeek()
        {
            int day = 0;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    day = 0;
                    break;
                case DayOfWeek.Tuesday:
                    day = 1;
                    break;
                case DayOfWeek.Wednesday:
                    day = 2;
                    break;
                case DayOfWeek.Thursday:
                    day = 3;
                    break;
                case DayOfWeek.Friday:
                    day = 4;
                    break;
                case DayOfWeek.Saturday:
                    day = 5;
                    break;
                case DayOfWeek.Sunday:
                    day = 6;
                    break;
                default:
                    break;
            }
            return day;
        }


    }
}
