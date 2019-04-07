using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimeLinePage : Page
    {
        public ObservableCollection<BangumiTimeLine> bangumiCollection { get; set; }

        public TimeLinePage()
        {
            this.InitializeComponent();
            bangumiCollection = new ObservableCollection<BangumiTimeLine>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (bangumiCollection.Count == 0)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Refresh();
                });
            }
        }

        /// <summary>
        /// 刷新时间表。
        /// </summary>
        public async void Refresh()
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;
            ClickToRefresh.Visibility = Visibility.Collapsed;
            if (await BangumiFacade.PopulateBangumiCalendarAsync(bangumiCollection))
            {
                UpdateTime.Text = "更新时间：" + DateTime.Now;
                WeekPivot.SelectedIndex = GetDayOfWeek();
            }
            else
            {
                UpdateTime.Text = "网络连接失败，请重试！";
            }
            ClickToRefresh.Visibility = Visibility.Visible;
            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 点击刷新。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Refresh();
            });
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Subject)e.ClickedItem;
            Frame.Navigate(typeof(DetailsPage), selectedItem);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = WeekPivot.ActualWidth - 24;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 200);
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
