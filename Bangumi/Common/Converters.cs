using Windows.UI.Xaml;

namespace Bangumi.Common
{
    /// <summary>
    /// Provides static methods for use in x:Bind function binding to convert bound values to the required value.
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Returns the reverse of the provided value.
        /// </summary>
        public static bool Not(bool value) => !value;

        /// <summary>
        /// Returns true if the specified value is not null; otherwise, returns false.
        /// </summary>
        public static bool IsNotNull(object value) => value != null;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified value is true; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIf(bool value) =>
            value ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified value is null; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNull(object value) =>
            value == null ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified string is null or empty; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNullOrEmpty(string value) =>
            string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;

        public static string GetSubjectTypeName(int type)
        {
            string cn = "";
            switch (type)
            {
                case 1:
                    cn = "书籍";
                    break;
                case 2:
                    cn = "动画";
                    break;
                case 3:
                    cn = "音乐";
                    break;
                case 4:
                    cn = "游戏";
                    break;
                case 6:
                    cn = "三次元";
                    break;
                default:
                    break;
            }
            return cn;
        }
    }
}
