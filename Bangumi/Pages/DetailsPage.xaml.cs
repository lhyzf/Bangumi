﻿using Bangumi.Models;
using Bangumi.Facades;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Bangumi.Helper;
using static Bangumi.Helper.OAuthHelper;
using System.Threading.Tasks;
using static Bangumi.Facades.BangumiFacade;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        public ObservableCollection<Ep> eps { get; set; }
        private static string subjectId = "";
        private static string imageSource = "";
        private static string name_cn = "";
        private static string air_date = "";
        private static int air_weekday = 0;

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
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDetails();
            await LoadEps();
        }

        private async Task LoadEps()
        {
            try
            {
                Progress progress = null;
                var subject = await BangumiFacade.GetSubjectEpsAsync(subjectId);
                var userId = await OAuthHelper.ReadFromFile(OAuthFile.user_id, false);
                if (!string.IsNullOrEmpty(userId))
                {
                    progress = await BangumiFacade.GetProgressesAsync(userId, subjectId);
                }

                eps.Clear();
                foreach (var ep in subject.eps)
                {
                    if (ep.status == "Air")
                    {
                        ep.status = "";
                        if (progress != null)
                        {
                            foreach (var p in progress.eps)
                            {
                                if (p.id == ep.id)
                                {
                                    ep.status = p.status.cn_name;
                                    break;
                                }
                                else
                                {
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

        private async Task LoadDetails()
        {
            Uri uri = new Uri(imageSource);
            ImageSource imgSource = new BitmapImage(uri);
            this.BangumiImage.Source = imgSource;
            this.NameTextBlock.Text = name_cn;
            if (!string.IsNullOrEmpty(air_date) || air_weekday != 0)
            {
                this.air_dateTextBlock.Text = "开播时间：" + air_date;
                this.air_weekdayTextBlock.Text = "更新时间：" + GetWeekday(air_weekday);
            }
            try
            {
                var details = new Subject();
                details = await BangumiFacade.GetSubjectAsync(subjectId);
                this.air_dateTextBlock.Text = "开播时间：" + details.air_date;
                this.air_weekdayTextBlock.Text = "更新时间：" + GetWeekday(details.air_weekday);
                var summary = "暂无简介";
                if (!string.IsNullOrEmpty(details.summary))
                {
                    if (details.summary.Length > 120)
                    {
                        summary = details.summary.Substring(0, 120) + "...";
                    }
                    else
                    {
                        summary = details.summary;
                    }
                }
                this.SummaryTextBlock.Text = summary;
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
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), StatusEnum.watched))
            {
                ep.status = "看过";
                //await LoadEps();
            }
        }

        // 看到
        private async void WatchedToMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressBatchAsync(ep.id, StatusEnum.watched, ep.sort))
            {
                await LoadEps();
            }
        }

        // 想看
        private async void QueueMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), StatusEnum.queue))
            {
                ep.status = "想看";
                //await LoadEps();
            }
        }

        // 抛弃
        private async void DropMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), StatusEnum.drop))
            {
                ep.status = "抛弃";
                //await LoadEps();
            }
        }

        // 未看
        private async void RemoveMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var ep = (Ep)EpsGridView.SelectedItem;
            if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), StatusEnum.remove))
            {
                ep.status = "";
                //await LoadEps();
            }
        }

        private void Eps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

    }
}
