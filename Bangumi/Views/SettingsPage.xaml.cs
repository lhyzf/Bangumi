using Bangumi.Helper;
using System;
using System.Linq;
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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            EpsBatchToggleSwitch.IsOn = SettingHelper.EpsBatch == true;
            ThemePanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == SettingHelper.MyTheme.ToString()).IsChecked = true;
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

        // 修改应用主题颜色
        private void OnThemeRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var selectedTheme = ((RadioButton)sender)?.Tag?.ToString();

            if (selectedTheme != null)
            {
                if (selectedTheme == "Dark")
                {
                    SettingHelper.MyTheme = ElementTheme.Dark;
                }
                else if (selectedTheme == "Light")
                {
                    SettingHelper.MyTheme = ElementTheme.Light;
                }
                else
                {
                    SettingHelper.MyTheme = ElementTheme.Default;
                }
            }
        }

    }
}
