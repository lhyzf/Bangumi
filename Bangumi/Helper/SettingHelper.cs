using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Bangumi.Helper
{
    static class SettingHelper
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static bool? _epsBatch;
        private static bool? _subjectComplete;
        private static bool? _useBangumiData;
        private static bool? _useBiliApp;

        static SettingHelper()
        {
            _epsBatch = localSettings.Values["EpsBatch"] as bool?;
            _subjectComplete = localSettings.Values["SubjectComplete"] as bool?;
            _useBangumiData = localSettings.Values["UseBangumiData"] as bool?;
            _useBiliApp = localSettings.Values["UseBiliApp"] as bool?;
        }

        public static bool EpsBatch
        {
            set
            {
                _epsBatch = value;
                localSettings.Values["EpsBatch"] = _epsBatch;
            }
            get
            {
                return _epsBatch == true;
            }
        }

        public static bool SubjectComplete
        {
            set
            {
                _subjectComplete = value;
                localSettings.Values["SubjectComplete"] = _subjectComplete;
            }
            get
            {
                return _subjectComplete == true;
            }
        }

        public static bool UseBangumiData
        {
            set
            {
                _useBangumiData = value;
                localSettings.Values["UseBangumiData"] = _useBangumiData;
            }
            get
            {
                return _useBangumiData == true;
            }
        }

        public static bool UseBiliApp
        {
            set
            {
                _useBiliApp = value;
                localSettings.Values["UseBiliApp"] = _useBiliApp;
            }
            get
            {
                return _useBiliApp == true;
            }
        }

    }
}
