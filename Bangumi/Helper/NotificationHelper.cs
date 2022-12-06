using Bangumi.Controls;

namespace Bangumi.Helper
{
    public static class NotificationHelper
    {
        public static void Notify(string msg, NotifyType notifyType = NotifyType.Message)
        {
            _ = MainPage.RootPage.NotifyControl.AddNotification(msg, notifyType);
        }

    }
}
