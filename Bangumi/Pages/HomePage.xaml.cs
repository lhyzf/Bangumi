using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static Bangumi.Helper.OAuthHelper;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<Watching> watchingCollection { get; set; }
        //public ObservableCollection<Progress> progressCollection { get; set; }

        public HomePage()
        {
            this.InitializeComponent();
            watchingCollection = new ObservableCollection<Watching>();

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (watchingCollection.Count == 0)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Refresh();
                });
            }
        }

        //刷新收视进度列表
        public async void Refresh()
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            ClickToRefresh.Visibility = Visibility.Collapsed;
            try
            {
                if (OAuthHelper.IsLogin)
                {
                    if (await BangumiFacade.PopulateWatchingListAsync(watchingCollection))
                    {
                        UpdateTime.Text = "更新时间：" + DateTime.Now;
                    }
                }
                else
                {
                    UpdateTime.Text = "请先登录！";
                }

            }
            catch (Exception)
            {
                UpdateTime.Text = "发生错误，请重试！";
            }
            ClickToRefresh.Visibility = Visibility.Visible;

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
        }

        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Refresh();
            });
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Watching)e.ClickedItem;
            Frame.Navigate(typeof(DetailsPage), selectedItem);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = DoingPivot.ActualWidth - 24;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 220);
        }
    }
}
