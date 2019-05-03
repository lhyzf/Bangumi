using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        public DetailsViewModel ViewModel { get; } = new DetailsViewModel();

        public DetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter.GetType() == typeof(Watching))
            {
                var p = (Watching)e.Parameter;
                ViewModel.SubjectId = p.subject_id.ToString();
                ViewModel.ImageSource = p.subject.images.common;
                ViewModel.Name_cn = p.subject.name_cn;
            }
            else if (e.Parameter.GetType() == typeof(Subject))
            {
                var p = (Subject)e.Parameter;
                ViewModel.SubjectId = p.id.ToString();
                ViewModel.ImageSource = p.images.common;
                ViewModel.Name_cn = p.name_cn;
                ViewModel.Air_date = p.air_date;
                ViewModel.Air_weekday = p.air_weekday;
                ViewModel.AirWeekdayName = ViewModel.GetWeekday(ViewModel.Air_weekday);
            }
            else if (e.Parameter.GetType() == typeof(Int32))
            {
                ViewModel.SubjectId = e.Parameter.ToString();
                ViewModel.ImageSource = ViewModel.NoImageUri;
            }

            ViewModel.LoadDetails();
            ViewModel.LoadCollectionStatus();
        }


        /// <summary>
        /// 章节看过。
        /// </summary>
        private void WatchedMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = EpsGridView.SelectedItem as Ep;
            ViewModel.UpdateEpStatus(ep, BangumiFacade.EpStatusEnum.watched);
        }

        /// <summary>
        /// 章节看到。
        /// </summary>
        private void WatchedToMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = EpsGridView.SelectedItem as Ep;
            ViewModel.UpdateEpStatusBatch(ep, BangumiFacade.EpStatusEnum.watched);
        }

        /// <summary>
        /// 章节想看。
        /// </summary>
        private void QueueMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = EpsGridView.SelectedItem as Ep;
            ViewModel.UpdateEpStatus(ep, BangumiFacade.EpStatusEnum.queue);
        }

        /// <summary>
        /// 章节抛弃。
        /// </summary>
        private void DropMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = EpsGridView.SelectedItem as Ep;
            ViewModel.UpdateEpStatus(ep, BangumiFacade.EpStatusEnum.drop);
        }

        /// <summary>
        /// 章节未看。
        /// </summary>
        private void RemoveMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = EpsGridView.SelectedItem as Ep;
            ViewModel.UpdateEpStatus(ep, BangumiFacade.EpStatusEnum.remove);
        }

        /// <summary>
        /// 修改章节状态弹出菜单。
        /// </summary>
        private void Eps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (OAuthHelper.IsLogin && !ViewModel.IsLoading && ((Ep)EpsGridView.SelectedItem).status != "NA")
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        /// <summary>
        /// 修改章节状态弹出菜单，无视章节状态。
        /// </summary>
        private void Eps_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (OAuthHelper.IsLogin && !ViewModel.IsLoading)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        /// <summary>
        /// 收藏想看。
        /// </summary>
        private void WishCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateCollectionStatus(BangumiFacade.CollectionStatusEnum.wish);
        }

        /// <summary>
        /// 收藏看过。
        /// </summary>
        private void WatchedCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateCollectionStatus(BangumiFacade.CollectionStatusEnum.collect);
        }

        /// <summary>
        /// 收藏在看。
        /// </summary>
        private void DoingCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateCollectionStatus(BangumiFacade.CollectionStatusEnum.@do);
        }

        /// <summary>
        /// 收藏搁置。
        /// </summary>
        private void OnHoldCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateCollectionStatus(BangumiFacade.CollectionStatusEnum.on_hold);
        }

        /// <summary>
        /// 收藏抛弃。
        /// </summary>
        private void DropCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateCollectionStatus(BangumiFacade.CollectionStatusEnum.dropped);
        }

        /// <summary>
        /// 收藏删除。
        /// 未提供 API。
        /// </summary>
        private void RemoveCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 编辑评分和吐槽。
        /// </summary>
        private void CollectionAdvanceButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditMyRate();
        }

        /// <summary>
        /// 更多资料。
        /// </summary>
        private async void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            SubjectMoreInfoContentDialog subjectMoreInfoContentDialog = new SubjectMoreInfoContentDialog()
            {
                name = ViewModel.name,
                info = ViewModel.moreInfo,
                summary = ViewModel.moreSummary,
                characters = ViewModel.moreCharacters,
                staff = ViewModel.moreStaff,
            };
            subjectMoreInfoContentDialog.Title = ViewModel.Name_cn;
            await subjectMoreInfoContentDialog.ShowAsync();
        }

        /// <summary>
        /// 在调整窗口大小时计算item的宽度。
        /// </summary>
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = EpsGridView.ActualWidth - EpsGridView.Padding.Left - EpsGridView.Padding.Right;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 200);
        }

    }
}
