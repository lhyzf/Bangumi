using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Bangumi.Api.Models;
using System.Collections.Generic;
using System.Linq;

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
        /// Returns Visibility.Collapsed if the specified value is not null; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNotNull(object value) =>
            value == null ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified value is not null; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNotZero(int value) =>
            value == 0 ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Returns Visibility.Collapsed if the specified string is null or empty; otherwise, returns Visibility.Visible.
        /// </summary>
        public static Visibility CollapsedIfNullOrEmpty(string value) =>
            string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// 返回条目类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取星期的中文
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static string GetWeekday(int day)
        {
            switch (day)
            {
                case 1:
                    return "星期一";
                case 2:
                    return "星期二";
                case 3:
                    return "星期三";
                case 4:
                    return "星期四";
                case 5:
                    return "星期五";
                case 6:
                    return "星期六";
                case 7:
                    return "星期日";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 根据观看进度返回相应描述
        /// </summary>
        /// <param name="watched_eps"></param>
        /// <returns></returns>
        public static string GetWatchedEpsDesc(int watched_eps)
        {
            if (watched_eps == 0)
                return "尚未观看";
            return "看到第" + watched_eps + "话";
        }

        /// <summary>
        /// 根据sort和type返回章节相应描述
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static string GetEpSortDesc(float sort, int type)
        {
            if (type == 0)
            {
                return "第 " + sort + " 话";
            }
            else
            {
                return type.GetEpisodeType() + " " + sort;
            }
        }

        /// <summary>
        /// 根据next_ep和eps返回非正片章节相应描述
        /// </summary>
        /// <param name="next_ep"></param>
        /// <returns></returns>
        public static string GetEpNextSortDesc(float next_ep, List<ViewModels.SimpleEp> eps)
        {
            if (next_ep == -1)
            {
                return "";
            }
            int? type = eps?.Where(p => p.sort == next_ep).FirstOrDefault()?.type;
            if (type == null)
            {
                return "";
            }
            else if (type == 0)
            {
                return next_ep.ToString();
            }
            else
            {
                return type?.GetEpisodeType() + " " + next_ep;
            }
        }

        /// <summary>
        /// 保留一位小数
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string DoubleToString(double d)
        {
            if (d == 0)
                return "-";
            else
                return d.ToString("0.0");
        }

    }
}
