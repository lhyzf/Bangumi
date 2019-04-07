using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CollectionPage : Page
    {
        public ObservableCollection<Collect> subjectCollection { get; set; }

        public CollectionPage()
        {
            this.InitializeComponent();
            subjectCollection = new ObservableCollection<Collect>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (subjectCollection.Count == 0 && !MyProgressRing.IsActive)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Refresh();
                });
            }
        }

        private async void TypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Refresh();
            });
        }

        /// <summary>
        /// 刷新收藏列表，API限制每类最多25条。
        /// </summary>
        public async void Refresh()
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;
            ClickToRefresh.Visibility = Visibility.Collapsed;
            var subjectType = GetSubjectType();
            if (OAuthHelper.IsLogin)
            {
                if (await BangumiFacade.PopulateSubjectCollectionAsync(subjectCollection, subjectType))
                {
                    UpdateTime.Text = "更新时间：" + DateTime.Now;
                }
                else
                {
                    UpdateTime.Text = "网络连接失败，请重试！";
                }
            }
            else
            {
                UpdateTime.Text = "请先登录！";
            }
            ClickToRefresh.Visibility = Visibility.Visible;
            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 点击刷新。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Refresh();
            });
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (SList)e.ClickedItem;
            Frame.Navigate(typeof(DetailsPage), selectedItem.subject);
        }

        private BangumiFacade.SubjectType GetSubjectType()
        {
            var type = TypeCombobox.SelectedIndex;
            BangumiFacade.SubjectType subjectType = BangumiFacade.SubjectType.anime;
            switch (type)
            {
                case 0:
                    subjectType = BangumiFacade.SubjectType.anime;
                    break;
                case 1:
                    subjectType = BangumiFacade.SubjectType.book;
                    break;
                case 2:
                    subjectType = BangumiFacade.SubjectType.music;
                    break;
                case 3:
                    subjectType = BangumiFacade.SubjectType.game;
                    break;
                case 4:
                    subjectType = BangumiFacade.SubjectType.real;
                    break;
                default:
                    break;
            }
            return subjectType;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = CollectionPivot.ActualWidth - 24;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 220);
        }
    }
}
