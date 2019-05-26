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
        public int rate { get; set; }
        public string comment { get; set; }
        public bool privacy { get; set; }
        public string collectionStatus { get; set; }

        public CollectionEditContentDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            rate = (int)RateSlider.Value;
            comment = CommentTextBox.Text;
            privacy = (bool)PrivacyCheckBox.IsChecked;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void StatusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            collectionStatus = (sender as RadioButton).Tag.ToString();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            StatusPanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == collectionStatus).IsChecked = true;
        }
    }
}
