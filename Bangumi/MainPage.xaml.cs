using Bangumi.Helper;
using Bangumi.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Bangumi
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SystemNavigationManager systemNavigationManager;
        public static MainPage rootPage;
        public static Frame rootFrame;
        public bool hasDialog = false;

        public MainPage()
        {
            this.InitializeComponent();
            rootPage = this;
            rootFrame = MyFrame;
            // 设置窗口的最小大小
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(300, 200));

            this.systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            CostomTitleBar();
        }

        /// <summary>
        /// 根据用户登录状态改变用户图标。
        /// </summary>
        /// <returns></returns>
        private async Task UpdataUserStatusAsync()
        {
            bool result = await OAuthHelper.CheckTokens();
            if (result)
            {
                LoginButton.Label = "注销";
                UserIcon.Glyph = "\uE7E8";
                rootFrame.Navigate(typeof(HomePage));
            }
            else
            {
                LoginButton.Label = "登录";
                UserIcon.Glyph = "\uEE57";
                rootFrame.Navigate(typeof(LoginPage));
                MyCommandBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 自定义标题栏
        /// </summary>
        private void CostomTitleBar()
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                // 将内容拓展到标题栏
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;
                // 设置标题栏按钮部分背景颜色
                var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
                appTitleBar.ButtonBackgroundColor = Colors.Transparent;
                appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                MainPage_ActualThemeChanged(null, "");
                ActualThemeChanged += MainPage_ActualThemeChanged;
            }
        }

        /// <summary>
        /// 在主题颜色改变时调用，设置标题栏按钮颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainPage_ActualThemeChanged(FrameworkElement sender, object args)
        {
            // 主题颜色改变时设置标题栏按钮颜色
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (ActualTheme == ElementTheme.Light)
                appTitleBar.ButtonForegroundColor = Colors.Black;
            else
                appTitleBar.ButtonForegroundColor = Colors.White;
        }

        /// <summary>
        /// 鼠标后退键返回上一页。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            var properties = args.CurrentPoint.Properties;

            // Ignore button chords with the left, right, and middle buttons
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed)
                return;

            // If back or foward are pressed (but not both) navigate appropriately
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                if (backPressed)
                    args.Handled = On_BackRequested();
                // if (forwardPressed) this.TryGoForward();
            }
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = On_BackRequested();
            }
        }

        // 页面向后导航
        private bool On_BackRequested()
        {
            if (!MainPage.rootFrame.CanGoBack)
                return false;

            // 有弹出框时不向后导航
            if (hasDialog)
                return false;

            // 处于首页时不向后导航
            if (MainPage.rootFrame.CurrentSourcePageType == typeof(HomePage))
                return false;

            MainPage.rootFrame.GoBack();
            return true;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdataUserStatusAsync();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            rootFrame.Navigate(typeof(SearchPage), null, new DrillInNavigationTransitionInfo());
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            rootFrame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
        }

        /// <summary>
        /// 点击登录按钮，根据登录状态执行不同操作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginButton.Label == "登录")
            {
                await UpdataUserStatusAsync();
            }
            else if (LoginButton.Label == "注销")
            {
                string choice = "";
                var msgDialog = new Windows.UI.Popups.MessageDialog("确定要退出登录吗？") { Title = "注销" };
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", uiCommand => { choice = uiCommand.Label; }));
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", uiCommand => { choice = uiCommand.Label; }));
                await msgDialog.ShowAsync();
                if (choice == "确定")
                {
                    OAuthHelper.DeleteTokens();
                    await UpdataUserStatusAsync();
                }
            }
        }
    }
}
