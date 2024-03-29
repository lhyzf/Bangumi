﻿using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.ViewModels;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CollectionPage : Page, IPageStatus
    {
        public CollectionViewModel ViewModel { get; } = new CollectionViewModel();

        public bool IsLoading => ViewModel.IsLoading;

        public async Task Refresh()
        {
            await ViewModel.PopulateSubjectCollectionAsync();
        }

        public CollectionPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!BangumiApi.BgmOAuth.IsLogin)
            {
                MainPage.RootPage.Frame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
                return;
            }
            if (ViewModel.SubjectCollection.Count == 0 && !ViewModel.IsLoading)
            {
                ViewModel.PopulateSubjectCollectionFromCache();
                await ViewModel.PopulateSubjectCollectionAsync();
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (SubjectBaseE)e.ClickedItem;
            this.Frame.Navigate(typeof(EpisodePage), selectedItem.SubjectId, new DrillInNavigationTransitionInfo());
        }

        // 更新条目收藏状态
        private async void UpdateCollectionStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                var sub = item.DataContext as SubjectBaseE;
                switch (item.Tag)
                {
                    case "Wish":
                        await ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.Wish);
                        break;
                    case "Collect":
                        await ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.Collect);
                        break;
                    case "Doing":
                        await ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.Do);
                        break;
                    case "OnHold":
                        await ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.OnHold);
                        break;
                    case "Dropped":
                        await ViewModel.UpdateCollectionStatus(sub, CollectionStatusType.Dropped);
                        break;
                    default:
                        break;
                }
            }
        }

        // 鼠标右键弹出菜单
        private void GridView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin
                && !ViewModel.IsLoading
                && e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                FrameworkElement element = e.OriginalSource switch
                {
                    GridViewItem item => item.ContentTemplateRoot as FrameworkElement,
                    FrameworkElement el => el,
                    _ => null
                };
                if (element != null && element.DataContext is SubjectBaseE)
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
            if (BangumiApi.BgmOAuth.IsLogin
                && !ViewModel.IsLoading
                && e.HoldingState == HoldingState.Started)
            {
                e.Handled = true;
                SetMenuFlyoutByType();
                CollectionMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        // 根据作品类别调整菜单文字
        private void SetMenuFlyoutByType()
        {
            switch (TypeCombobox.SelectedIndex)
            {
                case 1:
                    WishMenuFlyoutItem.Text = "想读";
                    CollectMenuFlyoutItem.Text = "读过";
                    DoingMenuFlyoutItem.Text = "在读";
                    break;
                case 2:
                    WishMenuFlyoutItem.Text = "想听";
                    CollectMenuFlyoutItem.Text = "听过";
                    DoingMenuFlyoutItem.Text = "在听";
                    break;
                case 3:
                    WishMenuFlyoutItem.Text = "想玩";
                    CollectMenuFlyoutItem.Text = "玩过";
                    DoingMenuFlyoutItem.Text = "在玩";
                    break;
                case 0:
                case 4:
                    WishMenuFlyoutItem.Text = "想看";
                    CollectMenuFlyoutItem.Text = "看过";
                    DoingMenuFlyoutItem.Text = "在看";
                    break;
                default:
                    WishMenuFlyoutItem.Text = "想做";
                    CollectMenuFlyoutItem.Text = "做过";
                    DoingMenuFlyoutItem.Text = "在做";
                    break;
            }
        }

    }
}
