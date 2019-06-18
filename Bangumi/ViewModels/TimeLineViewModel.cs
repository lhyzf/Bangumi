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

        /// <summary>
        /// 刷新时间表。
        /// </summary>
        public async void LoadTimeLine(bool force = false)
        {
            try
            {
                IsLoading = true;
                HomePage.homePage.isLoading = IsLoading;
                MainPage.rootPage.RefreshAppBarButton.IsEnabled = false;
                await BangumiFacade.PopulateBangumiCalendarAsync(BangumiCollection, force);
            }
            catch (Exception e)
            {
                var msgDialog = new Windows.UI.Popups.MessageDialog("获取时间表失败！\n" + e.Message) { Title = "错误！" };
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


    }
}
