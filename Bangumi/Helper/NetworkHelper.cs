using System;

namespace Bangumi.Helper
{
    public static class NetworkHelper
    {
        public static bool IsOffline => !Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;

        public static event EventHandler NetworkChanged;

        static NetworkHelper()
        {
            Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.NetworkChanged +=
                (sender, e) => NetworkChanged?.Invoke(sender, e);
        }
    }
}
