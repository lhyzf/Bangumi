using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Bangumi.Helper
{
    class SettingHelper
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static bool? EpsBatch
        {
            set
            {
                localSettings.Values["EpsBatch"] = value;
            }
            get
            {
                return localSettings.Values["EpsBatch"] as bool?;
            }
        }

        public static ElementTheme MyTheme
        {
            set
            {
                localSettings.Values["Theme"] = value.ToString();
            }
            get
            {
                string theme = localSettings.Values["Theme"] as string;
                if (theme == "Dark")
                    return ElementTheme.Dark;
                else if (theme == "Light")
                    return ElementTheme.Light;
                else
                    return ElementTheme.Default;
            }
        }
        
    }
}
