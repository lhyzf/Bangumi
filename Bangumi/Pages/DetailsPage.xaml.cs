using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
        private string airWeekdayName = "";

        // 更多资料用
        private string name;
        private string moreInfo;
        private string moreSummary;
        private string moreCharacters;
        private string moreStaff;

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
            if (e.Parameter.GetType() == typeof(Watching))
            {
                var p = (Watching)e.Parameter;
                subjectId = p.subject_id.ToString();
                imageSource = p.subject.images.common;
                name_cn = p.subject.name_cn;
                air_date = "";
                air_weekday = 0;
            }
            else if (e.Parameter.GetType() == typeof(Subject))
            {
                var p = (Subject)e.Parameter;
                subjectId = p.id.ToString();
                imageSource = p.images.common;
                name_cn = p.name_cn;
                air_date = p.air_date;
                air_weekday = p.air_weekday;
            }
            else if (e.Parameter.GetType() == typeof(Int32))
            {
                subjectId = e.Parameter.ToString();
                imageSource = "";
                name_cn = "";
                air_date = "";
                air_weekday = 0;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LoadDetails();
                LoadCollectionStatus();
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            EpsGridView.ItemsSource = null;
        }

        private void SetCollectionButton(string status)
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
            if (!OAuthHelper.IsLogin)
            {
                return;
            }
            try
            {
                CollectionStatus collectionStatus = await BangumiFacade.GetCollectionStatusAsync(subjectId);
                if (collectionStatus.status != null)
                {
                    SetCollectionButton(collectionStatus.status.name);
                    myRate = collectionStatus.rating;
                    myComment = collectionStatus.comment;
                    myPrivacy = collectionStatus.@private == "1" ? true : false;
                }
                else
                {
                    SetCollectionButton("");
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
            //if (!string.IsNullOrEmpty(imageSource))
            //{
            //    Uri uri = new Uri(imageSource);
            //    ImageSource imgSource = new BitmapImage(uri);
            //    this.BangumiImage.Source = imgSource;
            //}
            //this.NameTextBlock.Text = name_cn;
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
                    imageSource = subject.images.common;
                    //Uri uri = new Uri(subject.images.common);
                    //ImageSource imgSource = new BitmapImage(uri);
                    //this.BangumiImage.Source = imgSource;
                }
                this.air_dateTextBlock.Text = "开播时间：" + subject.air_date;
                this.air_weekdayTextBlock.Text = "更新时间：" + GetWeekday(subject.air_weekday);
                //airWeekdayName = GetWeekday(subject.air_weekday);
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

                // 更多资料
                name = subject.name;
                moreSummary = string.IsNullOrEmpty(subject.summary) ? "暂无简介" : subject.summary;
                moreInfo = "作品分类：" + GetSubjectTypeCn((BangumiFacade.SubjectType)subject.type);
                moreInfo += "\n放送开始：" + subject.air_date;
                moreInfo += "\n放送星期：" + GetWeekday(subject.air_weekday);
                moreInfo += "\n话数：" + subject.eps_count;
                if (subject.crt != null)
                {
                    foreach (var crt in subject.crt)
                    {
                        moreCharacters += string.Format("{0}：", string.IsNullOrEmpty(crt.name_cn) ? crt.name : crt.name_cn);
                        if (crt.actors != null)
                        {
                            foreach (var actor in crt.actors)
                            {
                                moreCharacters += actor.name + "、";
                            }
                            moreCharacters = moreCharacters.TrimEnd('、');
                        }
                        else
                        {
                            moreCharacters += "暂无资料";
                        }
                        moreCharacters += "\n";
                    }
                    moreCharacters = moreCharacters.TrimEnd('\n');
                }
                else
                {
                    moreCharacters += "暂无资料";
                }
                if (subject.staff != null)
                {
                    var sd = new Dictionary<string, string>();
                    foreach (var staff in subject.staff)
                    {
                        foreach (var job in staff.jobs)
                        {
                            if(!sd.ContainsKey(job))
                            {
                                sd.Add(job, string.IsNullOrEmpty(staff.name_cn) ? staff.name : staff.name_cn);
                            }
                            else
                            {
                                sd[job] += string.Format("、{0}", string.IsNullOrEmpty(staff.name_cn) ? staff.name : staff.name_cn);
                            }
                        }
                    }
                    foreach (var s in sd)
                    {
                        moreStaff += string.Format("{0}：{1}\n", s.Key, s.Value);
                    }
                    moreStaff = moreStaff.TrimEnd('\n');
                }
                else
                {
                    moreStaff += "暂无资料";
                }
                MoreInfoHyperLink.Visibility = Visibility.Visible;

                // 章节
                Progress progress = null;
                if (subject.eps == null)
                {
                    MyProgressRing.IsActive = false;
                    MyProgressRing.Visibility = Visibility.Collapsed;
                    return;
                }
                if (OAuthHelper.IsLogin)
                {
                    progress = await BangumiFacade.GetProgressesAsync(subjectId);
                }

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
            finally
            {
                MyProgressRing.IsActive = false;
                MyProgressRing.Visibility = Visibility.Collapsed;
            }
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
            if (!string.IsNullOrEmpty(OAuthHelper.AccessTokenString))
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        // 收藏 想看
        private async void WishCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await BangumiFacade.UpdateCollectionStatusAsync(subjectId, BangumiFacade.CollectionStatusEnum.wish))
            {
                SetCollectionButton("想看");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 看过
        private async void WatchedCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await BangumiFacade.UpdateCollectionStatusAsync(subjectId, BangumiFacade.CollectionStatusEnum.collect))
            {
                SetCollectionButton("看过");
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
            if (await BangumiFacade.UpdateCollectionStatusAsync(subjectId, BangumiFacade.CollectionStatusEnum.@do))
            {
                SetCollectionButton("在看");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 搁置
        private async void OnHoldCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await BangumiFacade.UpdateCollectionStatusAsync(subjectId, BangumiFacade.CollectionStatusEnum.on_hold))
            {
                SetCollectionButton("搁置");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 抛弃
        private async void DropCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await BangumiFacade.UpdateCollectionStatusAsync(subjectId, BangumiFacade.CollectionStatusEnum.dropped))
            {
                SetCollectionButton("抛弃");
            }
            MyProgressBar.Visibility = Visibility.Collapsed;
        }

        // 收藏 删除
        private async void RemoveCollectionFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MyProgressBar.Visibility = Visibility.Visible;
            if (await BangumiFacade.UpdateCollectionStatusAsync(subjectId, BangumiFacade.CollectionStatusEnum.collect))
            {
                SetCollectionButton("删除");
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
                CollectionAdvanceButton.IsEnabled = false;
                if (await BangumiFacade.UpdateCollectionStatusAsync(subjectId, GetStatusEnum(), collectionEditContentDialog.comment,
                     collectionEditContentDialog.rate.ToString(), collectionEditContentDialog.privacy == true ? "1" : "0"))
                {
                    myRate = collectionEditContentDialog.rate;
                    myComment = collectionEditContentDialog.comment;
                    myPrivacy = collectionEditContentDialog.privacy;
                }
                CollectionAdvanceButton.IsEnabled = true;
                MyProgressBar.Visibility = Visibility.Collapsed;
            }
        }


        // 更多资料
        private async void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            SubjectMoreInfoContentDialog subjectMoreInfoContentDialog = new SubjectMoreInfoContentDialog()
            {
                name = name,
                info = moreInfo,
                summary = moreSummary,
                characters = moreCharacters,
                staff = moreStaff,
            };
            subjectMoreInfoContentDialog.Title = name_cn;
            await subjectMoreInfoContentDialog.ShowAsync();
        }

        // 在调整窗口大小时计算item的宽度
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = EpsGridView.ActualWidth - EpsGridView.Padding.Left - EpsGridView.Padding.Right;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 200);
        }

        private string GetSubjectTypeCn(BangumiFacade.SubjectType type)
        {
            string cn = "";
            switch (type)
            {
                case BangumiFacade.SubjectType.book:
                    cn = "书籍";
                    break;
                case BangumiFacade.SubjectType.anime:
                    cn = "动画";
                    break;
                case BangumiFacade.SubjectType.music:
                    cn = "音乐";
                    break;
                case BangumiFacade.SubjectType.game:
                    cn = "游戏";
                    break;
                case BangumiFacade.SubjectType.real:
                    cn = "三次元";
                    break;
                default:
                    break;
            }
            return cn;
        }

        private string GetWeekday(int day)
        {
            var weekday = "";
            switch (day)
            {
                case 1:
                    weekday = "星期一";
                    break;
                case 2:
                    weekday = "星期二";
                    break;
                case 3:
                    weekday = "星期三";
                    break;
                case 4:
                    weekday = "星期四";
                    break;
                case 5:
                    weekday = "星期五";
                    break;
                case 6:
                    weekday = "星期六";
                    break;
                case 7:
                    weekday = "星期日";
                    break;
                default:
                    break;
            }
            return weekday;
        }

        private BangumiFacade.CollectionStatusEnum GetStatusEnum()
        {
            BangumiFacade.CollectionStatusEnum result;
            switch (CollectionButtonText.Text)
            {
                case "想看":
                    result = BangumiFacade.CollectionStatusEnum.wish;
                    break;
                case "看过":
                    result = BangumiFacade.CollectionStatusEnum.collect;
                    break;
                case "在看":
                    result = BangumiFacade.CollectionStatusEnum.@do;
                    break;
                case "搁置":
                    result = BangumiFacade.CollectionStatusEnum.on_hold;
                    break;
                case "抛弃":
                    result = BangumiFacade.CollectionStatusEnum.dropped;
                    break;
                default:
                    result = BangumiFacade.CollectionStatusEnum.@do;
                    break;
            }
            return result;
        }
    }
}
