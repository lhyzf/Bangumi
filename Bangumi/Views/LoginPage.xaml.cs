using Bangumi.Helper;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bangumi.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 10);
            await OAuthHelper.Authorize();
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 10);
            await Task.Delay(500);
            if (await OAuthHelper.CheckTokens())
                MainPage.rootFrame.Navigate(typeof(HomePage));
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.rootFrame.Navigate(typeof(HomePage));
        }

    }
}
