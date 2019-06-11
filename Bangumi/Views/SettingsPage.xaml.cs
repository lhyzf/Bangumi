using Bangumi.Helper;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public string Version
        {
            get
            {
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public SettingsPage()
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(GridTitleBar);
            // 启用标题栏的后退按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            MainPage.rootPage.MyCommandBar.Visibility = Visibility.Collapsed;

            EpsBatchToggleSwitch.IsOn = SettingHelper.EpsBatch == true;
            SubjectCompleteToggleSwitch.IsOn = SettingHelper.SubjectComplete == true;

            // 计算文件夹 JsonCache 中文件大小
            if (Directory.Exists(ApplicationData.Current.LocalCacheFolder.Path + "\\JsonCache"))
            {
                StorageFolder jsonCacheFolder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("JsonCache");
                var files = await jsonCacheFolder.GetFilesAsync();
                double fileSize = 0;
                foreach (var file in files)
                {
                    var fileInfo = await file.GetBasicPropertiesAsync();
                    fileSize += fileInfo.Size;
                }
                JsonCacheSizeTextBlock.Text = (fileSize / 1024).ToString("F3");
                DeleteUserCacheFileButton.IsEnabled = true;
            }
            else
            {
                // 文件夹 JsonCache 不存在
                JsonCacheSizeTextBlock.Text = "0";
                DeleteUserCacheFileButton.IsEnabled = false;
            }

            // 计算文件夹 ImageCache 中文件大小
            if (Directory.Exists(ApplicationData.Current.TemporaryFolder.Path + "\\ImageCache"))
            {
                StorageFolder imageCacheFolder = await ApplicationData.Current.TemporaryFolder.GetFolderAsync("ImageCache");
                var files = await imageCacheFolder.GetFilesAsync();
                double fileSize = 0;
                foreach (var file in files)
                {
                    var fileInfo = await file.GetBasicPropertiesAsync();
                    fileSize += fileInfo.Size;
                }
                ImageCacheSizeTextBlock.Text = (fileSize / 1024).ToString("F3");
                DeleteImageTempFileButton.IsEnabled = true;
            }
            else
            {
                // 文件夹 ImageCache 不存在
                ImageCacheSizeTextBlock.Text = "0";
                DeleteImageTempFileButton.IsEnabled = false;
            }
        }

        private void EpsBatchToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    SettingHelper.EpsBatch = true;
                }
                else
                {
                    SettingHelper.EpsBatch = false;
                }
            }
        }

        private void SubjectCompleteToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    SettingHelper.SubjectComplete = true;
                }
                else
                {
                    SettingHelper.SubjectComplete = false;
                }
            }
        }

        private async void DeleteJsonCacheFileButton_Click(object sender, RoutedEventArgs e)
        {
            // 删除Json缓存文件夹
            if (Directory.Exists(ApplicationData.Current.LocalCacheFolder.Path + "\\JsonCache"))
                await (await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("JsonCache")).DeleteAsync();
            JsonCacheSizeTextBlock.Text = "0";
            DeleteUserCacheFileButton.IsEnabled = false;
        }

        private async void DeleteImageTempFileButton_Click(object sender, RoutedEventArgs e)
        {
            // 删除图片缓存文件夹
            if (Directory.Exists(ApplicationData.Current.TemporaryFolder.Path + "\\ImageCache"))
                await (await ApplicationData.Current.TemporaryFolder.GetFolderAsync("ImageCache")).DeleteAsync();
            ImageCacheSizeTextBlock.Text = "0";
            DeleteImageTempFileButton.IsEnabled = false;
        }
    }
}
