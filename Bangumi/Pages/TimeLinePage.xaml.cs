﻿using Bangumi.Facades;
using Bangumi.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimeLinePage : Page
    {
        public ObservableCollection<BangumiCalendar> bangumiCollection { get; set; }

        public TimeLinePage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            bangumiCollection = new ObservableCollection<BangumiCalendar>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (bangumiCollection.Count == 0)
                await Refresh();
        }

        //刷新时间表
        public async Task Refresh()
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;
            
            ClickToRefresh.Visibility = Visibility.Collapsed;
            try
            {
                await BangumiFacade.PopulateBangumiCalendarAsync(bangumiCollection);
                UpdateTime.Text = "更新时间：" + DateTime.Now;
                switch (DateTime.Now.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        WeekPivot.SelectedIndex = 0;
                        break;
                    case DayOfWeek.Tuesday:
                        WeekPivot.SelectedIndex = 1;
                        break;
                    case DayOfWeek.Wednesday:
                        WeekPivot.SelectedIndex = 2;
                        break;
                    case DayOfWeek.Thursday:
                        WeekPivot.SelectedIndex = 3;
                        break;
                    case DayOfWeek.Friday:
                        WeekPivot.SelectedIndex = 4;
                        break;
                    case DayOfWeek.Saturday:
                        WeekPivot.SelectedIndex = 5;
                        break;
                    case DayOfWeek.Sunday:
                        WeekPivot.SelectedIndex = 6;
                        break;
                    default:
                        break;
                }
                var b = WeekPivot.SelectedItem;
            }
            catch (Exception)
            {
                UpdateTime.Text = "网络连接失败，请重试！";
            }
            ClickToRefresh.Visibility = Visibility.Visible;

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
        }

        //点击刷新
        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Refresh();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Item)e.ClickedItem;
            Frame.Navigate(typeof(DetailsPage), selectedItem.id);
        }

    }
}
