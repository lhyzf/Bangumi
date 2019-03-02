using Bangumi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimeLinePage : Page
    {
        public ObservableCollection<BangumiCalendar> bangumiCollection { get; set; }

        public TimeLinePage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            bangumiCollection = new ObservableCollection<BangumiCalendar>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (bangumiCollection.Count == 0)
                await Refresh();
        }

        //刷新时间表
        public async Task Refresh()
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;
            
            ClickToRefresh.Visibility = Visibility.Collapsed;
            await BangumiFacade.PopulateBangumiCalendarAsync(bangumiCollection);
            UpdateTime.Text = "更新时间：" + DateTime.Now;
            ClickToRefresh.Visibility = Visibility.Visible;

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
        }

        //点击刷新
        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Refresh();
        }

    }
}
