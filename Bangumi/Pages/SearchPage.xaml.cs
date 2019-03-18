using Bangumi.Facades;
using Bangumi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI.Core;
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
    public sealed partial class SearchPage : Page
    {
        public ObservableCollection<string> suggestions { get; set; }
        private bool suggestDelay = false;
        private string preAnime = "";
        private string preBook = "";
        private string preMusic = "";
        private string preGame = "";
        private string preReal = "";
        private bool searched = false;

        public SearchPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            suggestions=new ObservableCollection<string>();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedItem = (Subject)e.ClickedItem;
            Frame.Navigate(typeof(DetailsPage), selectedItem);
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            searched = false;
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                if (suggestDelay)
                {
                    return;
                }
                suggestDelay = true;
                if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && !string.IsNullOrEmpty(sender.Text))
                {
                    var input = sender.Text;
                    var result = await BangumiFacade.GetSearchResultAsync(input, "", 0, 10);
                    suggestions.Clear();
                    if (searched)
                    {
                        suggestDelay = false;
                        return;
                    }
                    if (result != null)
                    {
                        foreach (var item in result.list)
                        {
                            suggestions.Add(item.name_cn);
                        }
                    }
                }
                await Task.Delay(200);
                suggestDelay = false;
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                if (!CheckIfSearched())
                {
                    suggestions.Clear();
                    Search(args.ChosenSuggestion.ToString());
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
                if (!CheckIfSearched())
                {
                    suggestions.Clear();
                    Search(args.QueryText);
                }
            }
            // 使系统关闭虚拟键盘
            SearchBox.IsEnabled = false;
            SearchBox.IsEnabled = true;
        }

        private void TypePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CheckIfSearched())
            {
                suggestions.Clear();
                Search(SearchBox.Text);
            }
        }

        private bool CheckIfSearched()
        {
            bool flag = true;
            switch (TypePivot.SelectedIndex)
            {
                case 0:
                    if (AnimeGridView.Items.Count == 0 || SearchBox.Text != preAnime)
                    {
                        flag = false;
                        preAnime = SearchBox.Text;
                    }
                    break;
                case 1:
                    if (BookGridView.Items.Count == 0 || SearchBox.Text != preBook)
                    {
                        flag = false;
                        preBook = SearchBox.Text;
                    }
                    break;
                case 2:
                    if (MusicGridView.Items.Count == 0 || SearchBox.Text != preMusic)
                    {
                        flag = false;
                        preMusic = SearchBox.Text;
                    }
                    break;
                case 3:
                    if (GameGridView.Items.Count == 0 || SearchBox.Text != preGame)
                    {
                        flag = false;
                        preGame = SearchBox.Text;
                    }
                    break;
                case 4:
                    if (RealGridView.Items.Count == 0 || SearchBox.Text != preReal)
                    {
                        flag = false;
                        preReal = SearchBox.Text;
                    }
                    break;
            }
            return flag;
        }

        // 执行搜索操作
        private void Search(string keyword)
        {
            searched = true;
            if (string.IsNullOrEmpty(keyword))
            {
                return;
            }
            string type = "";
            switch (TypePivot.SelectedIndex)
            {
                case 0:
                    type = "2";
                    preAnime = keyword;
                    AnimeGridView.ItemsSource = new IncrementalLoadingCollection(keyword, type);
                    break;
                case 1:
                    type = "1";
                    preBook = keyword;
                    BookGridView.ItemsSource = new IncrementalLoadingCollection(keyword, type);
                    break;
                case 2:
                    type = "3";
                    preMusic = keyword;
                    MusicGridView.ItemsSource = new IncrementalLoadingCollection(keyword, type);
                    break;
                case 3:
                    type = "4";
                    preGame = keyword;
                    GameGridView.ItemsSource = new IncrementalLoadingCollection(keyword, type);
                    break;
                case 4:
                    type = "6";
                    preReal = keyword;
                    RealGridView.ItemsSource = new IncrementalLoadingCollection(keyword, type);
                    break;
            }
        }


    }

    // 分页加载
    class IncrementalLoadingCollection : ObservableCollection<Subject>, ISupportIncrementalLoading
    {
        int offset = 0;
        int max = 20;
        private string keyword;
        private string type;

        public IncrementalLoadingCollection(string keyword, string type)
        {
            this.keyword = keyword;
            this.type = type;
        }

        public bool HasMoreItems { get { return offset < max; } }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var dispatcher = Window.Current.Dispatcher;
            return AsyncInfo.Run(async cancelToken =>
            {
                System.Diagnostics.Debug.WriteLine("Loading {0}/{1} items", offset + 20, max);
                //await Task.Run(() => { });
                await Task.WhenAll(Task.Delay(1000), dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    SearchResult result = await BangumiFacade.GetSearchResultAsync(keyword, type, offset, 20);
                    if (result != null)
                    {
                        max = result.results;
                        foreach (Subject item in result.list)
                        {
                            Add(item);
                        }
                        if (!HasMoreItems)
                        {
                            System.Diagnostics.Debug.WriteLine("Loading complete.");
                        }
                    }
                }).AsTask());

                offset += 20;

                return new LoadMoreItemsResult { Count = count };
            });
        }
    }


}
