using Bangumi.Helper;
using Bangumi.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EpisodePage : Page, IPageStatus
    {
        public EpisodeViewModel ViewModel { get; private set; } = new EpisodeViewModel();

        public bool IsLoading => ViewModel.IsLoading;

        public async Task Refresh()
        {
            if (string.IsNullOrEmpty(ViewModel.SubjectId))
            {
                return;
            }
            await ViewModel.LoadDetails();
        }

        public EpisodePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (((Frame.Parent as NavigationView)?.Parent as Grid).Parent as MainPage)?.SelectPlaceholderItem("章节");

            if (int.TryParse(e.Parameter.ToString(), out _))
            {
                ViewModel.SubjectId = e.Parameter.ToString();
                ViewModel.InitViewModel();
                if (e.NavigationMode == NavigationMode.Back)
                {
                    ViewModel.LoadDetailsFromCache();
                }
                else
                {
                    await ViewModel.LoadDetails();
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void NavigateToDetailPage_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DetailPage), ViewModel.Detail);
        }

        private void ShareMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.Properties.Title = ViewModel.Name;
                dataPackage.Properties.Description = "——分享自 Bangumi UWP";
                switch (item.Tag)
                {
                    case "link":
                        dataPackage.SetText($"https://bgm.tv/subject/{ViewModel.SubjectId}");
                        Clipboard.SetContent(dataPackage);
                        NotificationHelper.Notify("条目链接已复制到剪贴板");
                        break;
                    case "id":
                        dataPackage.SetText(ViewModel.SubjectId);
                        Clipboard.SetContent(dataPackage);
                        NotificationHelper.Notify("条目ID已复制到剪贴板");
                        break;
                    case "system":
                        dataPackage.SetWebLink(new Uri($"https://bgm.tv/subject/{ViewModel.SubjectId}"));
                        var dataTransferManager = DataTransferManager.GetForCurrentView();
                        dataTransferManager.DataRequested += (s, args) =>
                          {
                              DataRequest request = args.Request;
                              request.Data = dataPackage;
                          };
                        DataTransferManager.ShowShareUI();
                        break;
                    default:
                        break;
                }
            }
        }

        private async void LaunchWebPage_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"https://bgm.tv/subject/{ViewModel.SubjectId}"));
        }
    }
}
