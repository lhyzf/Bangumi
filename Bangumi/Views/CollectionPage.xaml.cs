using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Api.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CollectionPage : Page
    {
        public CollectionViewModel ViewModel { get; } = new CollectionViewModel();

        public CollectionPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.rootPage.RefreshAppBarButton.Click += CollectionPageRefresh;
            if (ViewModel.SubjectCollection.Count == 0 && !ViewModel.IsLoading)
            {
                ViewModel.LoadCollectionList();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainPage.rootPage.RefreshAppBarButton.Click -= CollectionPageRefresh;
        }

        private void CollectionPageRefresh(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
            var tag = button.Tag;
            if (tag.Equals("收藏"))
                ViewModel.LoadCollectionList();
        }

        private void TypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedIndex = TypeCombobox.SelectedIndex;
            ViewModel.LoadCollectionList();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Subject2)e.ClickedItem;
            MainPage.rootFrame.Navigate(typeof(DetailsPage), selectedItem.Subject, new DrillInNavigationTransitionInfo());
        }

        // 右键弹出菜单
        private void ItemRelativePanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (OAuthHelper.IsLogin && !ViewModel.IsLoading)
            {
                CollectionMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        // 更新条目收藏状态
        private void UpdateCollectionStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            switch (item.Tag)
            {
                case "Wish":
                    ViewModel.UpdateCollectionStatus(item.DataContext as Subject2, CollectionStatusEnum.wish);
                    break;
                case "Collect":
                    ViewModel.UpdateCollectionStatus(item.DataContext as Subject2, CollectionStatusEnum.collect);
                    break;
                case "Doing":
                    ViewModel.UpdateCollectionStatus(item.DataContext as Subject2, CollectionStatusEnum.@do);
                    break;
                case "OnHold":
                    ViewModel.UpdateCollectionStatus(item.DataContext as Subject2, CollectionStatusEnum.on_hold);
                    break;
                case "Dropped":
                    ViewModel.UpdateCollectionStatus(item.DataContext as Subject2, CollectionStatusEnum.dropped);
                    break;
                default:
                    break;
            }
        }
    }
}
