using Bangumi.Helper;
using System;
using System.Linq;
using Windows.Storage;
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
        StorageFolder jsonCacheFolder;
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
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            EpsBatchToggleSwitch.IsOn = SettingHelper.EpsBatch == true;
            try
            {
                // 计算文件夹中文件大小
                jsonCacheFolder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("JsonCache");
                var files = await jsonCacheFolder.GetFilesAsync();
                double fileSize = 0;
                foreach (var file in files)
                {
                    var fileInfo = await file.GetBasicPropertiesAsync();
                    fileSize += fileInfo.Size;
                }
                CacheSizeTextBlock.Text = (fileSize / 1024).ToString("F3");
                DeleteTempFileButton.IsEnabled = true;
            }
            catch (Exception)
            {
                // 找不到文件夹
                CacheSizeTextBlock.Text = "0";
                DeleteTempFileButton.IsEnabled = false;
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

        private async void DeleteTempFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 删除Json缓存文件夹
                await jsonCacheFolder.DeleteAsync();
            }
            catch (Exception)
            {

            }
            finally
            {
                CacheSizeTextBlock.Text = "0";
                DeleteTempFileButton.IsEnabled = false;
            }
        }

    }
}
