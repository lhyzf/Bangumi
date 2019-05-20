﻿using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimeLinePage : Page
    {
        public TimeLineViewModel ViewModel { get; } = new TimeLineViewModel();

        public TimeLinePage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.rootPage.RefreshAppBarButton.Click += TimeLinePageRefresh;
            if (ViewModel.bangumiCollection.Count == 0 && !ViewModel.IsLoading)
            {
                ViewModel.LoadTimeLine();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainPage.rootPage.RefreshAppBarButton.Click -= TimeLinePageRefresh;
        }

        private void TimeLinePageRefresh(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
            var tag = button.Tag;
            if (tag.Equals("时间表"))
                ViewModel.LoadTimeLine();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Subject)e.ClickedItem;
            MainPage.rootFrame.Navigate(typeof(DetailsPage), selectedItem, new DrillInNavigationTransitionInfo());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = TimeLineSemanticZoom.ActualWidth - 24;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 200);
        }

    }
}
