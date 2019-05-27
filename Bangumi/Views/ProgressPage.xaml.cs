using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.Foundation;
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
    public sealed partial class ProgressPage : Page
    {
        public ProgressViewModel ViewModel { get; } = new ProgressViewModel();

        public ProgressPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.rootPage.RefreshAppBarButton.Click += ProgressPageRefresh;
            if (ViewModel.watchingCollection.Count == 0 && !ViewModel.IsLoading)
            {
                await ViewModel.LoadWatchingList();
                MyGridView.ScrollIntoView(ViewModel.watchingCollection[0]);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainPage.rootPage.RefreshAppBarButton.Click -= ProgressPageRefresh;
        }

        private async void ProgressPageRefresh(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
            var tag = button.Tag;
            if (tag.Equals("进度"))
            {
                await ViewModel.LoadWatchingList();
                MyGridView.ScrollIntoView(ViewModel.watchingCollection[0]);
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (WatchingStatus)e.ClickedItem;
            MainPage.rootFrame.Navigate(typeof(DetailsPage), selectedItem, new DrillInNavigationTransitionInfo());
            //Frame.Navigate(typeof(DetailsPage), selectedItem);
        }

        /// <summary>
        /// 在调整窗口大小时计算item的宽度。
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            double UseableWidth = availableSize.Width - MyGridView.Padding.Left - MyGridView.Padding.Right;
            if (UseableWidth > 0)
                MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 200);
            return base.MeasureOverride(availableSize);
        }

        // 将下一话标记为看过
        private void NextEpButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Windows.UI.Xaml.Controls.Button)sender;
            var item = (WatchingStatus)button.DataContext;
            ViewModel.UpdateEpStatus(item);
        }

        // 修改收藏状态
        private void CollectionButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Windows.UI.Xaml.Controls.Button)sender;
            var item = (WatchingStatus)button.DataContext;
            ViewModel.EditCollectionStatus(item);
        }
    }
}
