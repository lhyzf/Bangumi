using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
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
            NavigationCacheMode = NavigationCacheMode.Enabled;
            subjectCollection = new ObservableCollection<Collect>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (subjectCollection.Count == 0)
                Refresh();
        }

        // 刷新收藏列表，API限制每类最多25条
        public async void Refresh()
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            ClickToRefresh.Visibility = Visibility.Collapsed;
            try
            {
                var userId = await OAuthHelper.ReadFromFile(OAuthHelper.OAuthFile.user_id, false);
                var subjectType = GetSubjectType();
                if (!string.IsNullOrEmpty(userId))
                {
                    await BangumiFacade.PopulateSubjectCollectionAsync(subjectCollection, userId, subjectType);
                    UpdateTime.Text = "更新时间：" + DateTime.Now;
                }
                else
                {
                    UpdateTime.Text = "请先登录！";
                }
            }
            catch (Exception)
            {
                UpdateTime.Text = "网络连接失败，请重试！";
            }
            ClickToRefresh.Visibility = Visibility.Visible;

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
        }

        //点击刷新
        private void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Refresh();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (SList)e.ClickedItem;
            Frame.Navigate(typeof(DetailsPage), selectedItem.subject);
        }

        private void TypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        private BangumiFacade.SubjectType GetSubjectType()
        {
            var type = TypeCombobox.SelectedIndex;
            BangumiFacade.SubjectType subjectType = BangumiFacade.SubjectType.anime;
            switch (type)
            {
                case 0:subjectType = BangumiFacade.SubjectType.anime;
                    break;
                case 1:subjectType = BangumiFacade.SubjectType.book;
                    break;
                case 2:subjectType = BangumiFacade.SubjectType.music;
                    break;
                case 3:subjectType = BangumiFacade.SubjectType.game;
                    break;
                case 4:subjectType = BangumiFacade.SubjectType.real;
                    break;
                default:
                    break;
            }
            return subjectType;
        }
    }
}
