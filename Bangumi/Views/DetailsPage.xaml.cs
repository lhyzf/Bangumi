using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Api.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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
                if (!(ViewModel.SubjectId == p.subject_id.ToString()))
                {
                    needReLoad = true;
                    ViewModel.InitViewModel();
                }
                ViewModel.SubjectId = p.subject_id.ToString();
                ViewModel.ImageSource = p.image;
                ViewModel.NameCn = p.name_cn;
                if (p.eps != null)
                {
                    ViewModel.eps.Clear();
                    foreach (var ep in p.eps)
                    {
                        var newEp = new Ep();
                        newEp.Id = ep.id;
                        newEp.Sort = ep.sort;
                        newEp.Status = ep.status;
                        newEp.Type = ep.type;
                        newEp.NameCn = ep.name;
                        if (newEp.Type == 0)
                        {
                            newEp.Sort = "第 " + newEp.Sort + " 话";
                        }
                        else
                        {
                            newEp.Sort = ViewModel.GetEpisodeType(newEp.Type) + " " + newEp.Sort;
                        }
                        ViewModel.eps.Add(newEp);
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


        /// <summary>
        /// 修改章节状态。
        /// </summary>
        private void UpdateEpStatusMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var tag = item.Tag;
            var ep = EpsGridView.SelectedItem as Ep;
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
            if (OAuthHelper.IsLogin && !ViewModel.IsProgressLoading && ((Ep)EpsGridView.SelectedItem).Status != "NA")
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        /// <summary>
        /// 右键修改章节状态弹出菜单，无视章节状态。
        /// </summary>
        private void Eps_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (OAuthHelper.IsLogin && !ViewModel.IsProgressLoading)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        /// <summary>
        /// 更多资料。
        /// </summary>
        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowMoreInfo();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(GridTitleBar);
            // 启用标题栏的后退按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            // 设置刷新按钮可见以及事件绑定
            MainPage.rootPage.MyCommandBar.Visibility = Visibility.Visible;
            MainPage.rootPage.RefreshAppBarButton.Click += DetailPageRefresh;

            // 设置收藏按钮可见以及属性绑定、事件绑定
            Binding LabelBinding = new Binding
            {
                Source = ViewModel,
                Path = new PropertyPath("CollectionStatusText"),
            };
            MainPage.rootPage.CollectionAppBarButton.SetBinding(AppBarButton.LabelProperty, LabelBinding);
            Binding GlyphBinding = new Binding
            {
                Source = ViewModel,
                Path = new PropertyPath("CollectionStatusIcon"),
            };
            MainPage.rootPage.CollectionAppBarButtonFontIcon.SetBinding(FontIcon.GlyphProperty, GlyphBinding);
            Binding IsEnabledBinding = new Binding
            {
                Source = ViewModel,
                Path = new PropertyPath("IsStatusLoaded"),
            };
            MainPage.rootPage.CollectionAppBarButton.SetBinding(AppBarButton.IsEnabledProperty, IsEnabledBinding);
            MainPage.rootPage.CollectionAppBarButton.Click += CollectionAppBarButton_Click;
            MainPage.rootPage.CollectionAppBarButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 编辑评分和吐槽。
        /// </summary>
        private void CollectionAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditCollectionStatus();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // 设置收藏按钮隐藏以及解除事件绑定
            MainPage.rootPage.CollectionAppBarButton.Visibility = Visibility.Collapsed;
            MainPage.rootPage.CollectionAppBarButton.Click -= CollectionAppBarButton_Click;
            // 设置刷新按钮隐藏以及解除事件绑定
            MainPage.rootPage.RefreshAppBarButton.Click -= DetailPageRefresh;
        }

        private void DetailPageRefresh(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadDetails();
        }
    }
}
