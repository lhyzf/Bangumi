using Bangumi.Api;
using Bangumi.Api.Services;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e?.Parameter is string uri)
            {
                BitmapImage image = new BitmapImage { UriSource = new Uri(uri) };
                WelcomeImage.Source = image;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the URI
            string url = $"{BgmOAuth.OAuthHOST}/authorize?client_id={BangumiApi.BgmOAuth.ClientId}&response_type=code";
            var loginUri = new Uri(url);
            await Launcher.LaunchUriAsync(loginUri);
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
        }

    }
}
