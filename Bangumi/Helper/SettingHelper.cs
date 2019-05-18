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
        
    }
}
