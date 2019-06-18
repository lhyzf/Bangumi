using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Api.Models;
using Bangumi.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System.Profile;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchViewModel ViewModel { get; } = new SearchViewModel();
        ThreadPoolTimer DelayTimer;

        public SearchPage()
        {
            this.InitializeComponent();
            CostomTitleBar();
        }

        /// <summary>
        /// 自定义标题栏
        /// </summary>
        private void CostomTitleBar()
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            }
            else
            {
                GridTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 在标题栏布局变化时调用，修改左侧与右侧空白区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            GridTitleBar.Padding = new Thickness(
                sender.SystemOverlayLeftInset,
                0,
                sender.SystemOverlayRightInset,
                0);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Subject)e.ClickedItem;
            MainPage.rootFrame.Navigate(typeof(DetailsPage), selectedItem, new DrillInNavigationTransitionInfo());
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                 if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    if (DelayTimer != null && DelayTimer.Delay != TimeSpan.Zero)
                        DelayTimer.Cancel();
                    TimeSpan delay = TimeSpan.FromMilliseconds(1000);
                    DelayTimer = ThreadPoolTimer.CreateTimer(
                        async (source) =>
                        {
                            await Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                () =>
                                {
                                    ViewModel.GetSearchSuggestions();
                                });
                        },
                        delay);
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ViewModel.ResetSearchStatus();
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                ViewModel.SearchText = args.ChosenSuggestion.ToString();
                if (!ViewModel.CheckIfSearched())
                {
                    ViewModel.Suggestions.Clear();
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
                    ViewModel.Suggestions.Clear();
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
                ViewModel.Suggestions.Clear();
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
            string type = "";
            ViewModel.PreSearch[ViewModel.SelectedIndex] = ViewModel.SearchText;
            switch (ViewModel.SelectedIndex)
            {
                case 0:
                    type = "";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex);
                    AllGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 1:
                    type = "2";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex);
                    AnimeGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 2:
                    type = "1";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex);
                    BookGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 3:
                    type = "3";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex);
                    MusicGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 4:
                    type = "4";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex);
                    GameGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
                case 5:
                    type = "6";
                    ViewModel.SearchResultCollection = new SearchResultIncrementalLoadingCollection(ViewModel.SearchText, type, ViewModel.SelectedIndex);
                    RealGridView.ItemsSource = ViewModel.SearchResultCollection;
                    break;
            }
            ViewModel.SearchResultCollection.OnLoadMoreStarted += ViewModel.OnLoadMoreStarted;
            ViewModel.SearchResultCollection.OnLoadMoreCompleted += ViewModel.OnLoadMoreCompleted;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(GridTitleBar);
            // 启用标题栏的后退按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            MainPage.rootPage.MyCommandBar.Visibility = Visibility.Collapsed;
        }
    }
}
