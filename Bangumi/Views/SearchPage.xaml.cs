﻿using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Controls;
using Bangumi.Helper;
using Bangumi.ViewModels;
using System.Timers;
using Windows.Devices.Input;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchViewModel ViewModel { get; } = new SearchViewModel(DispatcherQueue.GetForCurrentThread());
        private readonly Timer delayTimer;

        public SearchPage()
        {
            InitializeComponent();
            delayTimer = new Timer(1000);
            delayTimer.Elapsed += async (sender, e) => await ViewModel.GetSearchSuggestions();
            delayTimer.AutoReset = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (((Frame.Parent as NavigationView)?.Parent as Grid).Parent as MainPage)?.SelectPlaceholderItem("搜索");
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (SubjectForSearch)e.ClickedItem;
            this.Frame.Navigate(typeof(EpisodePage), selectedItem.Id, new DrillInNavigationTransitionInfo());
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    delayTimer.Stop();
                    delayTimer.Start();
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ViewModel.ResetSearchStatus();
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                ViewModel.SearchText = args.ChosenSuggestion.ToString();
                if (!ViewModel.CheckIfSearched())
                {
                    ViewModel.Suggestions.Clear();
                    Search();
                }
            }
            else
            {
                // Use args.QueryText to determine what to do.
                int.TryParse(args.QueryText, out var result);
                if (result > 0)
                {
                    Frame.Navigate(typeof(EpisodePage), result, new DrillInNavigationTransitionInfo());
                }
                if (!ViewModel.CheckIfSearched())
                {
                    ViewModel.Suggestions.Clear();
                    Search();
                }
            }
            // 使系统关闭虚拟键盘
            SearchBox.IsEnabled = false;
            SearchBox.IsEnabled = true;
        }

        private void TypePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ViewModel.CheckIfSearched())
            {
                ViewModel.Suggestions.Clear();
                Search();
            }
        }

        /// <summary>
        /// 执行搜索操作。
        /// </summary>
        private void Search()
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (string.IsNullOrEmpty(ViewModel.SearchText))
            {
                return;
            }
            string type;
            ViewModel.PreSearch[ViewModel.SelectedIndex] = ViewModel.SearchText;
            switch (ViewModel.SelectedIndex)
            {
                case 0:
                    type = "";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex, DispatcherQueue.GetForCurrentThread());
                    AllGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 1:
                    type = "2";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex, DispatcherQueue.GetForCurrentThread());
                    AnimeGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 2:
                    type = "1";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex, DispatcherQueue.GetForCurrentThread());
                    BookGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 3:
                    type = "3";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex, DispatcherQueue.GetForCurrentThread());
                    MusicGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 4:
                    type = "4";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex, DispatcherQueue.GetForCurrentThread());
                    GameGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 5:
                    type = "6";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex, DispatcherQueue.GetForCurrentThread());
                    RealGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                default:
                    break;
            }
            ViewModel.SearchResultCollection.OnLoadMoreStarted += ViewModel.OnLoadMoreStarted;
            ViewModel.SearchResultCollection.OnLoadMoreCompleted += ViewModel.OnLoadMoreCompleted;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        // 鼠标右键弹出菜单
        private void GridView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin && e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                FrameworkElement element = e.OriginalSource switch
                {
                    GridViewItem item => item.ContentTemplateRoot as FrameworkElement,
                    FrameworkElement el => el,
                    _ => null
                };
                if (element != null && element.DataContext is SubjectForSearch)
                {
                    e.Handled = true;
                    SetMenuFlyoutByType();
                    CollectionMenuFlyout.ShowAt(element, e.GetPosition(element));
                }
            }
        }

        // 触摸长按弹出菜单
        private void ItemRelativePanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin && e.HoldingState == HoldingState.Started)
            {
                SetMenuFlyoutByType();
                CollectionMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        // 更新条目收藏状态
        private async void UpdateCollectionStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                switch (item.Tag)
                {
                    case "Wish":
                        await ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForSearch, CollectionStatusType.Wish);
                        break;
                    case "Collect":
                        await ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForSearch, CollectionStatusType.Collect);
                        break;
                    case "Doing":
                        await ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForSearch, CollectionStatusType.Do);
                        break;
                    case "OnHold":
                        await ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForSearch, CollectionStatusType.OnHold);
                        break;
                    case "Dropped":
                        await ViewModel.UpdateCollectionStatus(item.DataContext as SubjectForSearch, CollectionStatusType.Dropped);
                        break;
                    default:
                        break;
                }
            }
        }

        // 根据作品类别调整菜单文字
        private void SetMenuFlyoutByType()
        {
            switch (ViewModel.SelectedIndex)
            {
                case 2:
                    WishMenuFlyoutItem.Text = "想读";
                    CollectMenuFlyoutItem.Text = "读过";
                    DoingMenuFlyoutItem.Text = "在读";
                    break;
                case 3:
                    WishMenuFlyoutItem.Text = "想听";
                    CollectMenuFlyoutItem.Text = "听过";
                    DoingMenuFlyoutItem.Text = "在听";
                    break;
                case 4:
                    WishMenuFlyoutItem.Text = "想玩";
                    CollectMenuFlyoutItem.Text = "玩过";
                    DoingMenuFlyoutItem.Text = "在玩";
                    break;
                case 0:
                case 1:
                case 5:
                    WishMenuFlyoutItem.Text = "想看";
                    CollectMenuFlyoutItem.Text = "看过";
                    DoingMenuFlyoutItem.Text = "在看";
                    break;
                default:
                    break;
            }
        }

    }
}
