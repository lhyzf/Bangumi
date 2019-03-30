using Bangumi.Helper;
using Bangumi.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.System.Profile;
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

        public MainPage()
        {
            this.InitializeComponent();
            // 设置窗口的最小大小
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 480));
            // 启用标题栏的后退按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            // Handle keyboard and mouse navigation requests
            this.systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;

            //检测鼠标点击前进后退键
            Window.Current.CoreWindow.PointerPressed +=
                this.CoreWindow_PointerPressed;
        }

        //根据用户登录状态改变用户图标
        private async Task UpdataUserStatus()
        {
            bool result = await OAuthHelper.CheckTokens();
            if (result)
            {
                UserItem.Content = "注销";
                UserIcon.Glyph = "\uE7E8";
            }
            else
            {
                UserItem.Content = "登录";
                UserIcon.Glyph = "\uEE57";
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("home", typeof(HomePage)),
            ("collection", typeof(CollectionPage)),
            ("timeLine", typeof(TimeLinePage)),
            ("index", typeof(IndexPage)),
            ("search", typeof(SearchPage)),
        };

        private async void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdataUserStatus();

            // You can also add items in code.
            //NavView.MenuItems.Add(new muxc.NavigationViewItemSeparator());
            //NavView.MenuItems.Add(new muxc.NavigationViewItem
            //{
            //    Content = "My content",
            //    Icon = new SymbolIcon((Symbol)0xF1AD),
            //    Tag = "content"
            //});
            //_pages.Add(("content", typeof(MyContentPage)));

            // Add handler for ContentFrame navigation.
            ContentFrame.Navigated += On_Navigated;

            // NavView doesn't load any page by default, so load home page.
            NavView.SelectedItem = NavView.MenuItems[0];
            // If navigation occurs on SelectionChanged, this isn't needed.
            // Because we use ItemInvoked to navigate, we need to call Navigate
            // here to load the home page.
            NavView_Navigate("home", new EntranceNavigationTransitionInfo());

            if(AnalyticsInfo.VersionInfo.DeviceFamily== "Windows.Desktop")
            {
                // Add keyboard accelerators for backwards navigation.
                var goBack = new KeyboardAccelerator { Key = VirtualKey.GoBack };
                goBack.Invoked += BackInvoked;
                this.KeyboardAccelerators.Add(goBack);

                // ALT routes here
                var altLeft = new KeyboardAccelerator
                {
                    Key = VirtualKey.Left,
                    Modifiers = VirtualKeyModifiers.Menu
                };
                altLeft.Invoked += BackInvoked;
                this.KeyboardAccelerators.Add(altLeft);
            }

        }

        //鼠标后退键返回上一页
        private void CoreWindow_PointerPressed(CoreWindow sender,
            PointerEventArgs e)
        {
            var properties = e.CurrentPoint.Properties;

            // Ignore button chords with the left, right, and middle buttons
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed)
                return;

            // If back or foward are pressed (but not both) navigate appropriately
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                e.Handled = true;
                if (backPressed) this.On_BackRequested();
                // if (forwardPressed) this.TryGoForward();
            }
        }

        private void NavView_ItemInvoked(muxc.NavigationView sender,
                                         muxc.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null)
            {
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        /* NavView_SelectionChanged is not used in this example, but is shown for completeness.
             You will typically handle either ItemInvoked or SelectionChanged to perform navigation,
             but not both. */
        private void NavView_SelectionChanged(muxc.NavigationView sender,
                                              muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;
            if (navItemTag == "settings")
            {
                _page = typeof(SettingsPage);
            }
            else
            {
                var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                _page = item.Page;
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = On_BackRequested();
            }
        }

        private void NavView_BackRequested(muxc.NavigationView sender,
                                           muxc.NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }

        private void BackInvoked(KeyboardAccelerator sender,
                                 KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == muxc.NavigationViewDisplayMode.Compact ||
                 NavView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                NavView.SelectedItem = (muxc.NavigationViewItem)NavView.SettingsItem;
                NavView.Header = "设置";
            }
            else if (ContentFrame.SourcePageType == typeof(DetailsPage))
            {
                NavView.SelectedItem = null;
                NavView.Header = "详情";
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<muxc.NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));

                NavView.Header =
                    ((muxc.NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
            }
        }


        private async void NavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (UserItem.Content.ToString() == "登录")
            {
                MyProgressRing.IsActive = true;
                MyProgressRing.Visibility = Visibility.Visible;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 10);
                await OAuthHelper.Authorize();
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 10);
                MyProgressRing.IsActive = false;
                MyProgressRing.Visibility = Visibility.Collapsed;
                await UpdataUserStatus();
            }
            else if (UserItem.Content.ToString() == "注销")
            {
                string choice = "";
                var msgDialog = new Windows.UI.Popups.MessageDialog("确定要退出登录吗？") { Title = "注销" };
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", uiCommand => { choice = uiCommand.Label; }));
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", uiCommand => { choice = uiCommand.Label; }));
                await msgDialog.ShowAsync();
                if (choice == "确定")
                {
                    await OAuthHelper.DeleteTokens();
                    await UpdataUserStatus();
                }
            }
        }
    }
}
