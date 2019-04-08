using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Models;
using Bangumi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Bangumi.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        public SearchViewModel()
        {
        }

        public ObservableCollection<string> Suggestions { get; private set; } = new ObservableCollection<string>();
        public ObservableCollection<Subject> AllCollection;
        public ObservableCollection<Subject> AnimeCollection;
        public ObservableCollection<Subject> BookCollection;
        public ObservableCollection<Subject> MusicCollection;
        public ObservableCollection<Subject> GameCollection;
        public ObservableCollection<Subject> RealCollection;

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value);
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        private string _searchStatus;
        public string SearchStatus
        {
            get => _searchStatus;
            set => Set(ref _searchStatus, value);
        }

        public bool SuggestDelay = false;
        public string[] PreSearch = new string[6];

        /// <summary>
        /// 获取搜索建议。
        /// </summary>
        public async void GetSearchSuggestions()
        {
            if (SuggestDelay)
            {
                return;
            }
            SuggestDelay = true;
            if (!string.IsNullOrEmpty(SearchText))
            {
                var result = await BangumiFacade.GetSearchResultAsync(SearchText, "", 0, 10);
                Suggestions.Clear();
                if (SearchText == PreSearch[SelectedIndex])
                {
                    SuggestDelay = false;
                    return;
                }
                if (result != null)
                {
                    foreach (var item in result.list)
                    {
                        Suggestions.Add(item.name_cn);
                    }
                }
            }
            await Task.Delay(200);
            SuggestDelay = false;
        }

        /// <summary>
        /// 检查当前关键词是否与上一次搜索相同。
        /// </summary>
        /// <returns></returns>
        public bool CheckIfSearched()
        {
            if (SearchText == PreSearch[SelectedIndex])
            {
                return true;
            }
            else
            {
                PreSearch[SelectedIndex] = SearchText;
                return false;
            }
        }


    }

    /// <summary>
    /// 在页面到达底部时加载更多。
    /// </summary>
    public class SearchResultIncrementalLoadingCollection : ObservableCollection<Subject>, ISupportIncrementalLoading
    {
        public int offset { get; private set; } = 0;
        public int max { get; private set; } = 20;
        private string keyword;
        private string type;

        public SearchResultIncrementalLoadingCollection(string keyword, string type)
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
