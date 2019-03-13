using Bangumi.Models;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        public static ObservableCollection<Subject> subjectCollection;
        private static string subjectId;

        public DetailsPage()
        {
            this.InitializeComponent();
            subjectCollection = new ObservableCollection<Subject>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDetails();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            subjectId = e.Parameter.ToString();
        }

        private async void LoadDetails()
        {
            var details = new Subject();
            details = await BangumiFacade.GetSubjectAsync(subjectId);
            if (string.IsNullOrEmpty(details.name_cn))
            {
                details.name_cn = details.name;
            }
            Uri uri = new Uri(details.images.common);
            ImageSource imgSource = new BitmapImage(uri);
            this.BangumiImage.Source = imgSource;
            this.NameTextBlock.Text = details.name_cn;
            this.air_dateTextBlock.Text = "开播时间：" + details.air_date;
            var weekday = "";
            switch (details.air_weekday)
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
            this.air_weekdayTextBlock.Text = "更新时间：" + weekday;
            var summary = "暂无简介";
            if (!string.IsNullOrEmpty(details.summary))
            {
                if(details.summary.Length>120)
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
    }
}
