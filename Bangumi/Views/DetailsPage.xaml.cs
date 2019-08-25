﻿using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.ViewModels;
using System;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        public DetailsViewModel ViewModel { get; set; } = new DetailsViewModel();

        public DetailsPage()
        {
            this.InitializeComponent();
            CostomTitleBar();
        }

        /// <summary>
        /// 自定义标题栏
        /// </summary>
        private void CostomTitleBar()
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                CoreTitleBar_LayoutMetricsChanged(coreTitleBar, null);
                coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            }
            else
            {
                GridTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 在标题栏布局变化时调用，修改左侧与右侧空白区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            GridTitleBar.Padding = new Thickness(
                sender.SystemOverlayLeftInset,
                0,
                sender.SystemOverlayRightInset,
                0);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bool needReLoad = false;
            if (e.NavigationMode == NavigationMode.New)
            {
                needReLoad = true;
            }
            if (e.Parameter.GetType() == typeof(WatchingStatus))
            {
                var p = (WatchingStatus)e.Parameter;
                if (!(ViewModel.SubjectId == p.SubjectId.ToString()))
                {
                    needReLoad = true;
                    ViewModel.InitViewModel();
                }
                ViewModel.SubjectId = p.SubjectId.ToString();
                ViewModel.ImageSource = p.Image;
                ViewModel.NameCn = p.NameCn;
                ViewModel.AirDate = p.AirDate;
                ViewModel.AirWeekday = p.AirWeekday;
                ViewModel.CollectionStatusText = CollectionStatusEnum.Do.GetValue();
                ViewModel.CollectionStatusIcon = "\uE00B";
                if (p.Eps != null)
                {
                    ViewModel.Eps.Clear();
                    foreach (var ep in p.Eps)
                    {
                        var newEp = new Ep();
                        newEp.Id = ep.Id;
                        newEp.Sort = ep.Sort;
                        newEp.Status = ep.Status;
                        newEp.Type = ep.Type;
                        newEp.NameCn = ep.Name;
                        ViewModel.Eps.Add(newEp);
                    }
                }
            }
            else if (e.Parameter.GetType() == typeof(Subject))
            {
                var p = (Subject)e.Parameter;
                if (!(ViewModel.SubjectId == p.Id.ToString()))
                {
                    needReLoad = true;
                    ViewModel.InitViewModel();
                }
                ViewModel.SubjectId = p.Id.ToString();
                ViewModel.ImageSource = p.Images.Common;
                ViewModel.NameCn = p.NameCn;
                ViewModel.AirDate = p.AirDate;
                ViewModel.AirWeekday = p.AirWeekday;
            }
            else if (e.Parameter.GetType() == typeof(Int32))
            {
                if (!(ViewModel.SubjectId == e.Parameter.ToString()))
                {
                    needReLoad = true;
                    ViewModel.InitViewModel();
                }
                ViewModel.SubjectId = e.Parameter.ToString();
            }

            if (needReLoad)
            {
                ViewModel.LoadDetails();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // 设置收藏按钮隐藏以及解除事件绑定
            MainPage.RootPage.CollectionAppBarButton.Visibility = Visibility.Collapsed;
            MainPage.RootPage.CollectionAppBarButton.Click -= CollectionAppBarButton_Click;
            // 设置刷新按钮隐藏以及解除事件绑定
            MainPage.RootPage.RefreshAppBarButton.Click -= DetailPageRefresh_Click;
            // 设置访问网页按钮隐藏以及解除事件绑定
            MainPage.RootPage.WebPageAppBarButton.Visibility = Visibility.Collapsed;
            MainPage.RootPage.WebPageAppBarButton.Click -= LaunchWebPage_Click;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(GridTitleBar);
            // 启用标题栏的后退按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            // 设置刷新按钮可见以及事件绑定
            MainPage.RootPage.MyCommandBar.Visibility = Visibility.Visible;
            MainPage.RootPage.RefreshAppBarButton.Click += DetailPageRefresh_Click;

            // 设置访问网页按钮可见以及事件绑定
            MainPage.RootPage.WebPageAppBarButton.Visibility = Visibility.Visible;
            MainPage.RootPage.WebPageAppBarButton.Click += LaunchWebPage_Click;

            // 设置收藏按钮可见以及属性绑定、事件绑定
            if (BangumiApi.IsLogin)
            {
                // 标签文本描述
                Binding labelBinding = new Binding
                {
                    Source = ViewModel,
                    Path = new PropertyPath("CollectionStatusText"),
                };
                MainPage.RootPage.CollectionAppBarButton.SetBinding(AppBarButton.LabelProperty, labelBinding);
                MainPage.RootPage.CollectionAppBarButton.SetBinding(ToolTipService.ToolTipProperty, labelBinding);
                // 图标
                Binding glyphBinding = new Binding
                {
                    Source = ViewModel,
                    Path = new PropertyPath("CollectionStatusIcon"),
                };
                MainPage.RootPage.CollectionAppBarButtonFontIcon.SetBinding(FontIcon.GlyphProperty, glyphBinding);
                // 是否启用
                Binding isEnabledBinding = new Binding
                {
                    Source = ViewModel,
                    Path = new PropertyPath("IsStatusLoaded"),
                };
                MainPage.RootPage.CollectionAppBarButton.SetBinding(AppBarButton.IsEnabledProperty, isEnabledBinding);
                MainPage.RootPage.CollectionAppBarButton.Click += CollectionAppBarButton_Click;
                MainPage.RootPage.CollectionAppBarButton.Visibility = Visibility.Visible;
            }

            if (SettingHelper.UseBangumiData == true)
            {
                InitAirSites();
            }
            else
            {
                SitesMenuFlyout.Items.Clear();
                SelectedTextBlock.Text = "";
                SelectedTextBlock.DataContext = null;
            }
        }

        /// <summary>
        /// 修改章节状态。
        /// </summary>
        private void UpdateEpStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var tag = item.Tag;
            var ep = item.DataContext as Ep;
            switch (tag)
            {
                case "Watched":
                    ViewModel.UpdateEpStatus(ep, EpStatusEnum.watched);
                    break;
                case "WatchedTo":
                    ViewModel.UpdateEpStatusBatch(ep, EpStatusEnum.watched);
                    break;
                case "Queue":
                    ViewModel.UpdateEpStatus(ep, EpStatusEnum.queue);
                    break;
                case "Drop":
                    ViewModel.UpdateEpStatus(ep, EpStatusEnum.drop);
                    break;
                case "Remove":
                    ViewModel.UpdateEpStatus(ep, EpStatusEnum.remove);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 修改章节状态弹出菜单。
        /// </summary>
        private void Eps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (BangumiApi.IsLogin && !ViewModel.IsProgressLoading && ((sender as RelativePanel).DataContext as Ep).Status != "NA")
            {
                EpMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        /// <summary>
        /// 鼠标右键修改章节状态弹出菜单，无视章节状态。
        /// </summary>
        private void Eps_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (BangumiApi.IsLogin && !ViewModel.IsProgressLoading)
            {
                if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                {
                    EpMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
                }
            }
        }

        /// <summary>
        /// 触摸长按修改章节状态弹出菜单，无视章节状态。
        /// </summary>
        private void Eps_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (BangumiApi.IsLogin && !ViewModel.IsProgressLoading)
            {
                if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
                {
                    EpMenuFlyout.ShowAt((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
                }
            }
        }

        /// <summary>
        /// 编辑评分和吐槽。
        /// </summary>
        private void CollectionAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditCollectionStatus();
        }

        private void DetailPageRefresh_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadDetails();
        }

        private async void LaunchWebPage_Click(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            var uriWebPage = new Uri("https://bgm.tv/subject/" + ViewModel.SubjectId);

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uriWebPage);
        }

        private async void ItemsRepeater_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // The URI to launch
            var uriWebPage = new Uri((sender as RelativePanel).DataContext.ToString());

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uriWebPage);
        }

        private void RelativePanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var item = sender as RelativePanel;
            item.Background = Resources["ListViewItemBackgroundPointerOver"] as Windows.UI.Xaml.Media.SolidColorBrush;
            //Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 10);
        }

        private void RelativePanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var item = sender as RelativePanel;
            item.Background = Resources["ListViewItemBackgroundPressed"] as Windows.UI.Xaml.Media.SolidColorBrush;
            //Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 10);
        }

        private void RelativePanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var item = sender as RelativePanel;
            item.Background = Converters.ConvertBrushFromString("Transparent");
            //Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 10);
        }

        /// <summary>
        /// 点击拆分按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void SitesSplitButton_Click(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
        {
            var textBlock = sender.Content as TextBlock;
            var uri = textBlock.DataContext as string;
            await Launcher.LaunchUriAsync(new Uri(uri));
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
            SelectedTextBlock.Text = item.Text;
            SelectedTextBlock.DataContext = uri;
            await Launcher.LaunchUriAsync(new Uri(uri));
        }

        /// <summary>
        /// 初始化放送站点及拆分按钮
        /// </summary>
        private async void InitAirSites()
        {
            SitesMenuFlyout.Items.Clear();
            SelectedTextBlock.Text = "";
            SelectedTextBlock.DataContext = null;
            var airSites = await BangumiData.GetAirSitesByBangumiIdAsync(ViewModel.SubjectId);
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
                SelectedTextBlock.Text = airSites[0].SiteName;
                SelectedTextBlock.DataContext = airSites[0].Url;
            }
        }
    }
}
