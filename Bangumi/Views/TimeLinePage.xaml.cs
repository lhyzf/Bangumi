using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimeLinePage : Page
    {
        public TimeLineViewModel ViewModel { get; } = new TimeLineViewModel();

        public TimeLinePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ViewModel.bangumiCollection.Count == 0 && !ViewModel.IsLoading)
            {
                ViewModel.LoadTimeLine();
            }
        }

        private void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            ViewModel.LoadTimeLine();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Subject)e.ClickedItem;
            Frame.Navigate(typeof(DetailsPage), selectedItem);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = WeekPivot.ActualWidth - 24;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 200);
        }

    }
}
