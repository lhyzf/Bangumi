﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Bangumi.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System;

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
        /// Returns Visibility.Visible if the specified value is not null; otherwise, returns Visibility.Collapsed.
        /// </summary>
        public static Visibility CollapsedIfZero(int value) =>
            value == 0 ? Visibility.Collapsed : Visibility.Visible;

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
        /// 将js时间戳转换为C#时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertJsTickToDateTime(long dateTime)
        {
            // 默认 DateTime 使用 UTC 时间
            return new DateTime(1970, 1, 1).Add(new TimeSpan(long.Parse(dateTime + "0000000"))).ToLocalTime().ToString();
        }

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
            if (watched_eps == -1)
                return "";
            if (watched_eps == 0)
                return "尚未观看";
            return watched_eps.ToString();
        }

        /// <summary>
        /// 根据更新进度返回相应描述
        /// </summary>
        /// <param name="watched_eps"></param>
        /// <returns></returns>
        public static string GetUpdatedEpsDesc(int updated_eps)
        {
            if (updated_eps == -1)
                return "无章节";
            if (updated_eps == 0)
                return "尚未放送";
            return updated_eps.ToString();
        }

        /// <summary>
        /// 根据sort和type返回章节相应描述
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static string GetEpSortDesc(double sort, int type)
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
        public static string GetEpNextSortDesc(double next_ep, List<ViewModels.SimpleEp> eps)
        {
            if (next_ep == -1)
            {
                return "";
            }
            var ep = eps?.Where(p => p.Sort == next_ep).FirstOrDefault();
            if (ep == null)
            {
                return "";
            }
            else if (ep.Type == 0)
            {
                return next_ep.ToString() + 
                       (string.IsNullOrEmpty(ep.Name) ? "" : " " + ep.Name);
            }
            else
            {
                return ep.Type.GetEpisodeType() + " " + next_ep + 
                       (string.IsNullOrEmpty(ep.Name) ? "" : " " + ep.Name);
            }
        }

        /// <summary>
        /// 保留小数位数
        /// </summary>
        /// <param name="d"></param>
        /// <param name="n">小数点后几位</param>
        /// <returns></returns>
        public static string DoubleToString(double d, int n)
        {
            if (d == 0)
                return "-";
            else
                switch (n)
                {
                    case 0:
                        return d.ToString("0");
                    case 1:
                        return d.ToString("0.0");
                    case 2:
                        return d.ToString("0.00");
                    default:
                        return d.ToString();
                }
        }

        /// <summary>
        /// 将演员列表转为string
        /// </summary>
        /// <param name="actors"></param>
        /// <returns></returns>
        public static string ActorListToString(List<Actor> actors)
        {
            if (actors != null && actors.Count != 0)
            {
                string str = "CV：";
                foreach (var item in actors)
                {
                    str += item.Name + '、';
                }
                return str.TrimEnd('、');
            }
            else
                return "";
        }

        /// <summary>
        /// 将职责列表转为string
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static string JobListToString(List<string> jobs)
        {
            if (jobs != null && jobs.Count != 0)
            {
                string str = "";
                foreach (var item in jobs)
                {
                    str += item + '、';
                }
                return str.TrimEnd('、');
            }
            else
                return "";
        }

    }
}
