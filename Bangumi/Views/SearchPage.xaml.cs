using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchViewModel ViewModel { get; } = new SearchViewModel();

        public SearchPage()
        {
            this.InitializeComponent();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Subject)e.ClickedItem;
            Frame.Navigate(typeof(DetailsPage), selectedItem);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    ViewModel.GetSearchSuggestions();
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                ViewModel.SearchText = args.ChosenSuggestion.ToString();
                if (!ViewModel.CheckIfSearched())
                {
                    ViewModel.suggestions.Clear();
                    Search();
                }
            }
            else
            {
                // Use args.QueryText to determine what to do.
                int result = 0;
                int.TryParse(args.QueryText, out result);
                if (result > 0)
                {
                    Frame.Navigate(typeof(DetailsPage), result);
                }
                if (!ViewModel.CheckIfSearched())
                {
                    ViewModel.suggestions.Clear();
                    Search();
                }
            }
            // 使系统关闭虚拟键盘
            SearchBox.IsEnabled = false;
            SearchBox.IsEnabled = true;
        }

        private void TypePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ViewModel.CheckIfSearched())
            {
                ViewModel.suggestions.Clear();
                Search();
            }
        }

        /// <summary>
        /// 执行搜索操作。
        /// </summary>
        public void Search()
        {
            if (string.IsNullOrEmpty(ViewModel.SearchText))
            {
                return;
            }
            string keyword = ViewModel.SearchText;
            string type = "";
            switch (ViewModel.SelectedIndex)
            {
                case 0:
                    type = "2";
                    ViewModel.PreSearch[ViewModel.SelectedIndex] = ViewModel.SearchText;
                    AnimeGridView.ItemsSource = new SearchResultIncrementalLoadingCollection(keyword, type);
                    break;
                case 1:
                    type = "1";
                    ViewModel.PreSearch[ViewModel.SelectedIndex] = ViewModel.SearchText;
                    BookGridView.ItemsSource = new SearchResultIncrementalLoadingCollection(keyword, type);
                    break;
                case 2:
                    type = "3";
                    ViewModel.PreSearch[ViewModel.SelectedIndex] = ViewModel.SearchText;
                    MusicGridView.ItemsSource = new SearchResultIncrementalLoadingCollection(keyword, type);
                    break;
                case 3:
                    type = "4";
                    ViewModel.PreSearch[ViewModel.SelectedIndex] = ViewModel.SearchText;
                    GameGridView.ItemsSource = new SearchResultIncrementalLoadingCollection(keyword, type);
                    break;
                case 4:
                    type = "6";
                    ViewModel.PreSearch[ViewModel.SelectedIndex] = ViewModel.SearchText;
                    RealGridView.ItemsSource = new SearchResultIncrementalLoadingCollection(keyword, type);
                    break;
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double UseableWidth = TypePivot.ActualWidth - 20;
            MyWidth.Width = GridWidthHelper.GetWidth(UseableWidth, 200);
        }
    }



}
