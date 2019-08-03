using Bangumi.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        public bool _isLoading = false;
        public bool isLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static HomePage homePage;

        public HomePage()
        {
            this.InitializeComponent();
            homePage = this;
            CostomTitleBar();
#if DEBUG
            TitleTextBlock.Text += " (Debug)";
#endif
        }

        /// <summary>
        /// 自定义标题栏
        /// </summary>
        private void CostomTitleBar()
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

                CoreTitleBar_LayoutMetricsChanged(coreTitleBar, "");
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
                0,
                0,
                sender.SystemOverlayRightInset,
                0);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(GridTitleBar);
            // 禁用标题栏的后退按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            MainPage.RootPage.MyCommandBar.Visibility = Visibility.Visible;
            MainPage.RootPage.RefreshAppBarButton.IsEnabled = true;

            if (OAuthHelper.IsLogin)
            {
                if (!HomePagePivot.Items.Cast<PivotItem>().Any(p => p.Name == "CollectionItem"))
                {
                    HomePagePivot.Items.Insert(0, CollectionItem);
                }
                if (!HomePagePivot.Items.Cast<PivotItem>().Any(p => p.Name == "ProgressItem"))
                {
                    HomePagePivot.Items.Insert(0, ProgressItem);
                    HomePagePivot.SelectedIndex = 0;
                    ProgressPageFrame.Navigate(typeof(ProgressPage), null, new SuppressNavigationTransitionInfo());
                }
            }
            else
            {
                // 在进度页与收藏页任一页面存在时运行
                if (HomePagePivot.Items.Cast<PivotItem>().Any(p => p.Name == "ProgressItem")
                    || HomePagePivot.Items.Cast<PivotItem>().Any(p => p.Name == "CollectionItem"))
                {
                    HomePagePivot.Items.Remove(ProgressItem);
                    HomePagePivot.Items.Remove(CollectionItem);
                    TimeLinePageFrame.Navigate(typeof(TimeLinePage), null, new SuppressNavigationTransitionInfo());
                    MainPage.RootPage.RefreshAppBarButton.Tag = TimeLineItem.Header;
                }
            }
        }

        private void HomePagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            var pivotItem = pivot.SelectedItem as PivotItem;
            var frame = pivotItem.Content as Frame;
            MainPage.RootPage.RefreshAppBarButton.Tag = pivotItem.Header;
            if (frame.Content == null)
            {
                switch (pivotItem.Header)
                {
                    case "进度":
                        ProgressPageFrame.Navigate(typeof(ProgressPage), null, new SuppressNavigationTransitionInfo());
                        break;
                    case "收藏":
                        CollectionPageFrame.Navigate(typeof(CollectionPage), null, new SuppressNavigationTransitionInfo());
                        break;
                    case "时间表":
                        TimeLinePageFrame.Navigate(typeof(TimeLinePage), null, new SuppressNavigationTransitionInfo());
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
