using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Bangumi.ContentDialogs
{
    public sealed partial class CollectionEditContentDialog : ContentDialog
    {
        public int Rate { get; set; }
        public string Comment { get; set; }
        public bool Privacy { get; set; }
        public string CollectionStatus { get; set; }

        public CollectionEditContentDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Rate = (int)RateSlider.Value;
            Comment = CommentTextBox.Text;
            Privacy = (bool)PrivacyCheckBox.IsChecked;
        }

        private void StatusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CollectionStatus = (sender as RadioButton).Tag.ToString();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (CollectionStatus != "收藏")
                StatusPanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == CollectionStatus).IsChecked = true;
        }
    }
}
