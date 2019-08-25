using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Api.Models;
using Bangumi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bangumi.Helper;

namespace Bangumi.ViewModels
{
    public class TimeLineViewModel : ViewModelBase
    {
        public TimeLineViewModel()
        {
            IsLoading = false;
        }

        public ObservableCollection<BangumiTimeLine> BangumiCollection { get; private set; } = new ObservableCollection<BangumiTimeLine>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                Set(ref _isUpdating, value);
            }
        }

        /// <summary>
        /// 刷新时间表。
        /// </summary>
        public async void LoadTimeLine(bool force = false)
        {
            try
            {
                IsLoading = true;
                HomePage.homePage.isLoading = IsLoading;
                MainPage.RootPage.RefreshAppBarButton.IsEnabled = false;
                await BangumiFacade.PopulateBangumiCalendarAsync(BangumiCollection, force);
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("获取时间表失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
                HomePage.homePage.isLoading = IsLoading;
                MainPage.RootPage.RefreshAppBarButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 更新条目的收藏状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="collectionStatus"></param>
        public async void UpdateCollectionStatus(Subject subject, CollectionStatusEnum collectionStatus)
        {
            if (subject != null)
            {
                IsUpdating = true;
                await BangumiFacade.UpdateCollectionStatusAsync(subject.Id.ToString(), collectionStatus);
                IsUpdating = false;
            }
        }

    }
}
