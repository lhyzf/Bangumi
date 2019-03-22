using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static Bangumi.Facades.BangumiFacade;
using static Bangumi.Helper.OAuthHelper;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        public ObservableCollection<Ep> eps { get; set; }
        private string subjectId = "";
        private string imageSource = "";
        private string name_cn = "";
        private string air_date = "";
        private int air_weekday = 0;

        // 评分、吐槽用
        private int myRate;
        private string myComment;
        private bool myPrivacy;

        public DetailsPage()
        {
            this.InitializeComponent();
            eps = new ObservableCollection<Ep>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter.GetType().Name.Equals("Watching"))
            {
                var p = (Watching)e.Parameter;
                subjectId = p.subject_id.ToString();
                imageSource = p.subject.images.common;
                name_cn = p.subject.name_cn;
                air_date = "";
                air_weekday = 0;
            }
            else if (e.Parameter.GetType().Name.Equals("Subject"))
            {
                var p = (Subject)e.Parameter;
                subjectId = p.id.ToString();
                imageSource = p.images.common;
                name_cn = p.name_cn;
                air_date = p.air_date;
                air_weekday = p.air_weekday;
            }
            else if (e.Parameter.GetType().Name.Equals("Int32"))
            {
                subjectId = e.Parameter.ToString();
                imageSource = "";
                name_cn = "";
                air_date = "";
                air_weekday = 0;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            LoadDetails();
            LoadCollectionStatus();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            EpsGridView.ItemsSource = null;
        }

        private void SetCollectionButon(string status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "抛弃")
                {
                    CollectionButtonIcon.Glyph = "\uE007";
                }
                else
                {
                    CollectionButtonIcon.Glyph = "\uE00B";
                }
                CollectionButtonText.Text = status;
            }
            else
            {
                CollectionButtonIcon.Glyph = "\uE006";
                CollectionButtonText.Text = "收藏";
            }
        }

        // 获取收藏、评分和吐槽信息
        private async void LoadCollectionStatus()
        {
            try
            {
                CollectionStatus collectionStatus = await GetCollectionStatusAsync(subjectId);
                if (collectionStatus.status != null)
                {
                    SetCollectionButon(collectionStatus.status.name);
                    myRate = collectionStatus.rating;
                    myComment = collectionStatus.comment;
                    myPrivacy = collectionStatus.@private == "1" ? true : false;
                }
                else
                {
                    SetCollectionButon("");
                }
                CollectionButton.IsEnabled = true;
                CollectionAdvanceButton.IsEnabled = true;
            }
            catch (Exception e)
            {
                var msgDialog = new Windows.UI.Popups.MessageDialog(e.Message) { Title = "Error" };
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                await msgDialog.ShowAsync();
            }
        }

        // 加载详情和章节
        private async void LoadDetails()
        {
            if (!string.IsNullOrEmpty(imageSource))
            {
                Uri uri = new Uri(imageSource);
                ImageSource imgSource = new BitmapImage(uri);
                this.BangumiImage.Source = imgSource;
            }
            this.NameTextBlock.Text = name_cn;
            if (!string.IsNullOrEmpty(air_date) || air_weekday != 0)
            {
                this.air_dateTextBlock.Text = "开播时间：" + air_date;
                this.air_weekdayTextBlock.Text = "更新时间：" + GetWeekday(air_weekday);
            }
            try
            {
                // 条目信息
                var subject = await BangumiFacade.GetSubjectAsync(subjectId);
                if (string.IsNullOrEmpty(imageSource))
                {
                    Uri uri = new Uri(subject.images.common);
                    ImageSource imgSource = new BitmapImage(uri);
                    this.BangumiImage.Source = imgSource;
                }
                this.air_dateTextBlock.Text = "开播时间：" + subject.air_date;
                this.air_weekdayTextBlock.Text = "更新时间：" + GetWeekday(subject.air_weekday);
                var summary = "暂无简介";
                if (!string.IsNullOrEmpty(subject.summary))
                {
                    if (subject.summary.Length > 80)
                    {
                        summary = subject.summary.Substring(0, 80) + "...";
                    }
                    else
                    {
                        summary = subject.summary;
                    }
                }
                this.SummaryTextBlock.Text = summary;

                // 章节
                Progress progress = null;
                if (subject.eps == null)
                {
                    MyProgressRing.IsActive = false;
                    MyProgressRing.Visibility = Visibility.Collapsed;
                    return;
                }
                var userId = await OAuthHelper.ReadFromFile(OAuthFile.user_id, false);
                if (!string.IsNullOrEmpty(userId))
                {
                    progress = await BangumiFacade.GetProgressesAsync(userId, subjectId);
                }

                MyProgressRing.IsActive = false;
                MyProgressRing.Visibility = Visibility.Collapsed;
                eps.Clear();
                foreach (var ep in subject.eps)
                {
                    if (ep.status == "Air")
                    {
                        ep.status = "";
                        if (string.IsNullOrEmpty(ep.name_cn))
                        {
                            ep.name_cn = ep.name;
                        }
                        if (progress != null)
                        {
                            foreach (var p in progress.eps)
                            {
                                if (p.id == ep.id)
                                {
                                    ep.status = p.status.cn_name;
                                    break;
                                }
                            }
                        }
                        eps.Add(ep);
                    }
                }

            }
            catch (Exception e)
            {
                var msgDialog = new Windows.UI.Popups.MessageDialog(e.Message) { Title = "Error" };
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                await msgDialog.ShowAsync();
            }
        }

        private string GetWeekday(int day)
        {
            var weekday = "";
            switch (day)
            {
                case 1:
                    weekday = "周一";
                    break;
                case 2:
                    weekday = "周二";
                    break;
                case 3:
                    weekday = "周三";
                    break;
                case 4:
                    weekday = "周四";
                    break;
                case 5:
                    weekday = "周五";
                    break;
                case 6:
                    weekday = "周六";
                    break;
                case 7:
                    weekday = "周日";
                    break;
                default:
                    break;
            }
            return weekday;
        }

        // 看过
        private async void WatchedMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), BangumiFacade.EpStatusEnum.watched))
            {
                ep.status = "看过";
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 看到
        private async void WatchedToMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            var ep = (Ep)EpsGridView.SelectedItem;
            string epsId = string.Empty;
            foreach (var episode in eps)
            {
                if (episode.id == ep.id)
                {
                    epsId += episode.id.ToString();
                    break;
                }
                else
                {
                    epsId += episode.id.ToString() + ",";
                }
            }
            if (await BangumiFacade.UpdateProgressBatchAsync(ep.id, BangumiFacade.EpStatusEnum.watched, epsId))
            {
                foreach (var episode in eps)
                {
                    episode.status = "看过";
                    if (episode.id == ep.id)
                    {
                        break;
                    }
                }
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 想看
        private async void QueueMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), BangumiFacade.EpStatusEnum.queue))
            {
                ep.status = "想看";
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 抛弃
        private async void DropMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), BangumiFacade.EpStatusEnum.drop))
            {
                ep.status = "抛弃";
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 未看
        private async void RemoveMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), BangumiFacade.EpStatusEnum.remove))
            {
                ep.status = "";
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        private void Eps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        // 收藏 想看
        private async void WishCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await UpdateCollectionStatusAsync(subjectId, CollectionStatusEnum.wish))
            {
                SetCollectionButon("想看");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 看过
        private async void WatchedCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await UpdateCollectionStatusAsync(subjectId, CollectionStatusEnum.collect))
            {
                SetCollectionButon("看过");
                int epId = 0; ;
                string epsId = string.Empty;
                foreach (var episode in eps)
                {
                    if (eps.IndexOf(episode) == eps.Count - 1)
                    {
                        epsId += episode.id.ToString();
                        epId = episode.id;
                        break;
                    }
                    else
                    {
                        epsId += episode.id.ToString() + ",";
                    }
                }
                if (await BangumiFacade.UpdateProgressBatchAsync(epId, BangumiFacade.EpStatusEnum.watched, epsId))
                {
                    foreach (var episode in eps)
                    {
                        episode.status = "看过";
                    }
                }

            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 在看
        private async void DoingCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await UpdateCollectionStatusAsync(subjectId, CollectionStatusEnum.@do))
            {
                SetCollectionButon("在看");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 搁置
        private async void OnHoldCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await UpdateCollectionStatusAsync(subjectId, CollectionStatusEnum.on_hold))
            {
                SetCollectionButon("搁置");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 抛弃
        private async void DropCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await UpdateCollectionStatusAsync(subjectId, CollectionStatusEnum.dropped))
            {
                SetCollectionButon("抛弃");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 删除
        private async void RemoveCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await UpdateCollectionStatusAsync(subjectId, CollectionStatusEnum.collect))
            {
                SetCollectionButon("删除");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 编辑评分和吐槽
        private async void CollectionAdvanceButton_Click(object sender, RoutedEventArgs e)
        {
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog()
            {
                rate = myRate,
                comment = myComment,
                privacy = myPrivacy,
            };
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync())
            {
                MyProgressBar.Visibility = Visibility.Visible;
                if (await UpdateCollectionStatusAsync(subjectId, GetStatusEnum(), collectionEditContentDialog.comment,
                     collectionEditContentDialog.rate.ToString(), collectionEditContentDialog.privacy == true ? "1" : "0"))
                {
                    myRate = collectionEditContentDialog.rate;
                    myComment = collectionEditContentDialog.comment;
                    myPrivacy = collectionEditContentDialog.privacy;
                }
                MyProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private CollectionStatusEnum GetStatusEnum()
        {
            CollectionStatusEnum result;
            switch (CollectionButtonText.Text)
            {
                case "想看":
                    result = CollectionStatusEnum.wish;
                    break;
                case "看过":
                    result = CollectionStatusEnum.collect;
                    break;
                case "在看":
                    result = CollectionStatusEnum.@do;
                    break;
                case "搁置":
                    result = CollectionStatusEnum.on_hold;
                    break;
                case "抛弃":
                    result = CollectionStatusEnum.dropped;
                    break;
                default:
                    result = CollectionStatusEnum.@do;
                    break;
            }
            return result;
        }


        // 在调整窗口大小时计算item的宽度
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = EpsGridView.ActualWidth - EpsGridView.Margin.Left - EpsGridView.Margin.Right;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 200);
        }

        // 更多资料
        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
