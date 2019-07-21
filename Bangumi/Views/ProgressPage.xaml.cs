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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Bangumi.Data;
using System.Linq;
using System.Threading.Tasks;

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
            if (ViewModel.WatchingCollection.Count == 0 && !ViewModel.IsLoading)
            {
                await ViewModel.LoadWatchingListAsync();
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
                await ViewModel.LoadWatchingListAsync();
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (WatchingStatus)e.ClickedItem;
            MainPage.rootFrame.Navigate(typeof(DetailsPage), selectedItem, new DrillInNavigationTransitionInfo());
        }

        // 将下一话标记为看过
        private void NextEpButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Windows.UI.Xaml.Controls.Button)sender;
            var item = (WatchingStatus)button.DataContext;
            ViewModel.UpdateNextEpStatus(item);
        }

        // 修改收藏状态
        private void CollectionButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Windows.UI.Xaml.Controls.Button)sender;
            var item = (WatchingStatus)button.DataContext;
            ViewModel.EditCollectionStatus(item);
        }

        /// <summary>
        /// 点击菜单项打开站点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SiteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var uri = item.DataContext as string;
            await Launcher.LaunchUriAsync(new Uri(uri));
        }

        /// <summary>
        /// 初始化放送站点及拆分按钮
        /// </summary>
        private async Task InitAirSites(string id)
        {
            SitesMenuFlyout.Items.Clear();
            var airSites = await BangumiDataHelper.GetAirSitesByBangumiID(id);
            if (airSites.Count != 0)
            {
                foreach (var site in airSites)
                {
                    MenuFlyoutItem menuFlyoutItem = new MenuFlyoutItem()
                    {
                        Text = site.SiteName,
                        DataContext = site.Url
                    };
                    menuFlyoutItem.Click += SiteMenuFlyoutItem_Click;
                    SitesMenuFlyout.Items.Add(menuFlyoutItem);
                }
            }
            else
            {
                MenuFlyoutItem menuFlyoutItem = new MenuFlyoutItem()
                {
                    Text = "无放送站点"
                };
                SitesMenuFlyout.Items.Add(menuFlyoutItem);
            }
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RelativePanel_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if(SettingHelper.UseBangumiData==true)
            {
                var panel = sender as RelativePanel;
                var watch = panel.DataContext as WatchingStatus;
                await InitAirSites(watch.SubjectId.ToString());
                SitesMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }
    }
}
