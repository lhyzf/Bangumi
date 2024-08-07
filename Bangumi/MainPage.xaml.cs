﻿using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.Data;
using Bangumi.Helper;
using Bangumi.Views;
using Microsoft.Toolkit.Uwp;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Bangumi
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 检查属性值是否相同。
        /// 仅在不同时设置属性值。
        /// </summary>
        /// <typeparam name="T">属性类型。</typeparam>
        /// <param name="storage">可读写的属性。</param>
        /// <param name="value">属性值。</param>
        /// <param name="propertyName">属性名。可被支持 CallerMemberName 的编译器自动提供。</param>
        /// <returns>值改变则返回 true，未改变返回 false。</returns>
        private bool Set<T>(ref T storage, T value,
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        public bool IsLoading
        {
            get => (ContentFrame.Content as IPageStatus)?.IsLoading ?? false;
        }
        public void PageStatusChanged()
        {
            OnPropertyChanged(nameof(IsRefreshable));
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(CanGoBack));
        }

        public bool IsOffline => NetworkHelper.IsOffline;

        private bool _canGoBack;
        public bool CanGoBack
        {
            get => _canGoBack;
            private set => Set(ref _canGoBack, value);
        }

        public bool IsRefreshable => ContentFrame.Content is IPageStatus;

        public static MainPage RootPage { get; private set; }
        public bool HasDialog = false;

        public MainPage()
        {
            this.InitializeComponent();
#if DEBUG
            TitleBar.Text += " (Debug)";
#endif
            RootPage = this;
            // 设置窗口的最小大小
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 200));
            // 标题栏后退按钮
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) =>
            {
                if (!e.Handled)
                {
                    e.Handled = TryGoBack();
                }
            };
            // 鼠标后退按钮
            Window.Current.CoreWindow.PointerPressed += (sender, args) =>
            {
                if (args.CurrentPoint.Properties.IsXButton1Pressed)
                {
                    args.Handled = TryGoBack();
                }
            };

            RefreshButton.Click += (sender, e) => (ContentFrame.Content as IPageStatus)?.Refresh();
            SearchButton.Click += (sender, e) => NavigateToPage(typeof(SearchPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
            SettingButton.Click += (sender, e) => NavigateToPage(typeof(SettingsPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });

            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            NetworkHelper.NetworkChanged += (sender, e) => dispatcherQueue.EnqueueAsync(() => OnPropertyChanged(nameof(IsOffline)));

            // 初始化 Api 对象
            BangumiApi.Init(
                Constants.ClientId,
                Constants.ClientSecret,
                Constants.RedirectUrl,
                ApplicationData.Current.LocalFolder.Path,
                ApplicationData.Current.LocalCacheFolder.Path,
                EncryptionHelper.EncryptionAsync,
                EncryptionHelper.DecryptionAsync);

            if (SettingHelper.UseBangumiData)
            {
                // 初始化 BangumiData 对象
                BangumiData.Init(Path.Combine(ApplicationData.Current.LocalFolder.Path, "bangumi-data"),
                                 SettingHelper.UseBiliApp,
                                 (message) => NotificationHelper.Notify(message));
            }

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.BackStack.Clear();
            UpdateBackButtonStatus();
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            if (BangumiApi.BgmOAuth.IsLogin)
            {
                NavigateToPage(typeof(ProgressPage), null, new SuppressNavigationTransitionInfo());
            }
            else
            {
                NavigateToPage(typeof(CalendarPage), null, new SuppressNavigationTransitionInfo());
            }
            await UpdateAvatar();
        }

        /// <summary>
        /// 页面向后导航
        /// </summary>
        /// <returns></returns>
        private bool TryGoBack()
        {
            if (!ContentFrame.CanGoBack)
            {
                return false;
            }
            // 有弹出框时不向后导航
            if (HasDialog)
            {
                return false;
            }
            // 处于登录页时不向后导航
            if (this.Frame.CurrentSourcePageType == typeof(LoginPage))
            {
                return false;
            }
            ContentFrame.GoBack();
            return true;
        }

        private void UpdateBackButtonStatus()
        {
            CanGoBack = ContentFrame.CanGoBack && !(this.Frame.CurrentSourcePageType == typeof(LoginPage));
        }

        /// <summary>
        /// 点击登录按钮，根据登录状态执行不同操作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (BangumiApi.BgmOAuth.IsLogin)
            {
                string choice = "";
                var msgDialog = new Windows.UI.Popups.MessageDialog("确定要退出登录吗？") { Title = "注销" };
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", uiCommand => { choice = uiCommand.Label; }));
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", uiCommand => { choice = uiCommand.Label; }));
                await msgDialog.ShowAsync();
                if (choice == "确定")
                {
                    BangumiApi.BgmOAuth.DeleteUserFiles();
                    this.Frame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
                }
            }
            else
            {
                this.Frame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
            }
        }

        private async Task UpdateAvatar()
        {
            AvaterImage.ImageSource = null;
            BitmapImage img;
            if (BangumiApi.BgmOAuth.IsLogin && !NetworkHelper.IsOffline)
            {
                try
                {
                    var user = await BangumiApi.BgmApi.User();
                    ToolTipService.SetToolTip(LoginButton, $"{user.NickName}({user.UserName}@{user.Id})");
                    img = new BitmapImage(new Uri(user.Avatar.Small));
                }
                catch (Exception)
                {
                    ToolTipService.SetToolTip(LoginButton, null);
                    img = new BitmapImage(new Uri(Constants.NoAvatarImgUri));
                }
            }
            else
            {
                img = new BitmapImage(new Uri(Constants.NoAvatarImgUri));
            }
            AvaterImage.ImageSource = img;
        }

        public void ResetFrameBackStack()
        {
            NavigateToPage(typeof(ProgressPage), null, null);
            ContentFrame.BackStack.Clear();
        }

        public void NavigateToPage(Type type, object parameter, NavigationTransitionInfo transitionInfo)
        {
            if (type == null || ContentFrame.CurrentSourcePageType == type)
            {
                return;
            }
            ContentFrame.Navigate(type, parameter, transitionInfo);
        }

        public void SelectPlaceholderItem(string title)
        {
            PlaceholderItem.Content = title;
            PlaceholderItem.Visibility = Visibility.Visible;
            NavView.SelectedItem = PlaceholderItem;
        }

        private async Task HidePlaceholderItem()
        {
            await Task.Delay(500);
            if (!PlaceholderItem.IsSelected)
            {
                PlaceholderItem.Visibility = Visibility.Collapsed;
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Type type = args.InvokedItemContainer.Tag switch
            {
                "progress" => typeof(ProgressPage),
                "collection" => typeof(CollectionPage),
                "calendar" => typeof(CalendarPage),
                _ => null
            };
            NavigateToPage(type, null, args.RecommendedNavigationTransitionInfo);
        }

        private async void ContentFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            UpdateBackButtonStatus();
            PageStatusChanged();
            if (e.SourcePageType == typeof(ProgressPage))
            {
                NavView.SelectedItem = NavView.MenuItems[0];
                await HidePlaceholderItem();
            }
            else if (e.SourcePageType == typeof(CollectionPage))
            {
                NavView.SelectedItem = NavView.MenuItems[1];
                await HidePlaceholderItem();
            }
            else if (e.SourcePageType == typeof(CalendarPage))
            {
                NavView.SelectedItem = NavView.MenuItems[2];
                await HidePlaceholderItem();
            }
            else
            {
                NavView.SelectedItem = PlaceholderItem;
            }
        }

    }

}
