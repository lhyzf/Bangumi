using Bangumi.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Bangumi.Controls
{
    public sealed partial class SimpleSubject : UserControl
    {
        public EpisodeViewModel ViewModel
        {
            get { return (EpisodeViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(SimpleSubject), typeof(EpisodeViewModel), typeof(SimpleSubject), null);


        public SimpleSubject()
        {
            this.InitializeComponent();
        }

    }
}
