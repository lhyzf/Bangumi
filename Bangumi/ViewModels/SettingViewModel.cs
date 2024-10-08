﻿using Bangumi.Api;
using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Controls;
using Bangumi.Data;
using Bangumi.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Bangumi.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private bool _versionCheckFailed = false;

        public List<Url> Urls { get; set; } = new List<Url>
        {
            new Url {Uri="https://github.com/bangumi/api", Desc="Bangumi API(https://github.com/bangumi/api)"},
            new Url {Uri="https://github.com/Microsoft/microsoft-ui-xaml", Desc="WinUI 2(https://github.com/Microsoft/microsoft-ui-xaml)"},
            new Url {Uri="https://github.com/CommunityToolkit/WindowsCommunityToolkit", Desc="WindowsCommunityToolkit(https://github.com/CommunityToolkit/WindowsCommunityToolkit)"},
            new Url {Uri="https://github.com/bangumi-data/bangumi-data", Desc="bangumi-data(https://github.com/bangumi-data/bangumi-data)"},
            new Url {Uri="https://flurl.dev", Desc="Flurl(https://flurl.dev)"},
            new Url {Uri="https://github.com/Tlaster/WeiPo", Desc="WeiPo(https://github.com/Tlaster/WeiPo)"},
            new Url {Uri="https://github.com/sovetskyfish/pixivfs-uwp", Desc="pixivfs-uwp(https://github.com/sovetskyfish/pixivfs-uwp)"},
            new Url {Uri="https://github.com/App-vNext/Polly", Desc="Polly(https://github.com/App-vNext/Polly)"},
        };

        public string Version => string.Format("版本：{0} v{1}.{2}.{3}.{4} {5}",
                                    Package.Current.DisplayName,
                                    Package.Current.Id.Version.Major,
                                    Package.Current.Id.Version.Minor,
                                    Package.Current.Id.Version.Build,
                                    Package.Current.Id.Version.Revision,
                                    Package.Current.Id.Architecture);

        public string PackageName => string.Format("包名：{0}", Package.Current.Id.Name);
        public string InstalledDate => string.Format("安装时间：{0}", Package.Current.InstalledDate.ToLocalTime().DateTime);

        public bool EpsBatch
        {
            get => SettingHelper.EpsBatch;
            set
            {
                SettingHelper.EpsBatch = value;
                OnPropertyChanged();
            }
        }

        public bool SubjectComplete
        {
            get => SettingHelper.SubjectComplete;
            set
            {
                SettingHelper.SubjectComplete = value;
                OnPropertyChanged();
            }
        }

        public bool OrderByAirTime
        {
            get => SettingHelper.OrderByAirTime;
            set
            {
                SettingHelper.OrderByAirTime = value;
                OnPropertyChanged();
            }
        }

        public bool EnableBackgroundTask
        {
            get
            {
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    if (cur.Value.Name == Constants.RefreshTokenTask)
                    {
                        return true;
                    }
                }
                return false;
            }
            set
            {
                if (value)
                {
                    if (!BangumiApi.BgmOAuth.IsLogin)
                    {
                        NotificationHelper.Notify("请先登录！", NotifyType.Warn);
                        OnPropertyChanged();
                        return;
                    }

                    // If the user denies access, the task will not run.
                    var requestTask = BackgroundExecutionManager.RequestAccessAsync();

                    var builder = new BackgroundTaskBuilder();

                    builder.Name = Constants.RefreshTokenTask;

                    builder.SetTrigger(new TimeTrigger((uint)TimeSpan.FromDays(1).TotalMinutes, false));

                    builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                    // If the condition changes while the background task is executing then it will
                    // be canceled.
                    builder.CancelOnConditionLoss = true;

                    builder.Register();

                }
                else
                {
                    EnableBangumiAirToast = false;
                    // 取消所有后台任务
                    foreach (var cur in BackgroundTaskRegistration.AllTasks)
                    {
                        cur.Value.Unregister(true);
                    }
                }

                OnPropertyChanged();
            }
        }

        public bool UseBangumiData
        {
            get => SettingHelper.UseBangumiData;
            set
            {
                SettingHelper.UseBangumiData = value;
                if (value)
                {
                    // 获取数据版本
                    BangumiData.Init(Path.Combine(ApplicationData.Current.LocalFolder.Path, "bangumi-data"),
                                     SettingHelper.UseBiliApp);
                }
                else
                {
                    BangumiData.AutoCheck = false;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(BangumiDataVersion));
                OnPropertyChanged(nameof(UseBangumiDataAirSites));
                OnPropertyChanged(nameof(UseBiliApp));
                OnPropertyChanged(nameof(UseBangumiDataAirTime));
                OnPropertyChanged(nameof(BangumiDataAutoCheck));
                OnPropertyChanged(nameof(BangumiDataCheckInterval));
                OnPropertyChanged(nameof(BangumiDataAutoUpdate));
            }
        }

        public bool BangumiDataAutoCheck
        {
            get => BangumiData.AutoCheck;
            set
            {
                BangumiData.AutoCheck = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BangumiDataCheckInterval));
                OnPropertyChanged(nameof(BangumiDataAutoUpdate));
            }
        }

        public int BangumiDataCheckInterval
        {
            get => BangumiData.CheckInterval;
            set
            {
                BangumiData.CheckInterval = value;
                OnPropertyChanged();
            }
        }

        public bool BangumiDataAutoUpdate
        {
            get => BangumiData.AutoUpdate;
            set
            {
                BangumiData.AutoUpdate = value;
                OnPropertyChanged();
            }
        }


        public bool UseBangumiDataAirSites
        {
            get => SettingHelper.UseBangumiDataAirSites;
            set
            {
                SettingHelper.UseBangumiDataAirSites = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseBiliApp));
            }
        }

        public bool UseBiliApp
        {
            get => SettingHelper.UseBiliApp;
            set
            {
                SettingHelper.UseBiliApp = value;
                BangumiData.UseBiliApp = value;
                OnPropertyChanged();
            }
        }

        public bool UseBangumiDataAirTime
        {
            get => SettingHelper.UseBangumiDataAirTime;
            set
            {
                SettingHelper.UseBangumiDataAirTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableBangumiAirToast));
                OnPropertyChanged(nameof(UseActionCenterMode));
            }
        }

        public bool EnableBangumiAirToast
        {
            get => SettingHelper.EnableBangumiAirToast;
            set
            {
                if (value)
                {
                    if (!EnableBackgroundTask)
                    {
                        NotificationHelper.Notify("该选项需启用后台任务后可用", NotifyType.Warn);
                        value = false;
                    }
                }
                else
                {
                    UseActionCenterMode = false;
                    ToastNotificationHelper.RemoveAllScheduledToasts();
                }
                SettingHelper.EnableBangumiAirToast = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseActionCenterMode));
            }
        }

        public bool UseActionCenterMode
        {
            get
            {
                var v = BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(Constants.ToastBackgroundTask));
                SettingHelper.UseActionCenterMode = v;
                return v;
            }
            set
            {
                if (value)
                {
                    // If the user denies access, the task will not run.
                    var requestTask = BackgroundExecutionManager.RequestAccessAsync();

                    var builder = new BackgroundTaskBuilder();

                    builder.Name = Constants.ToastBackgroundTask;

                    builder.SetTrigger(new ToastNotificationActionTrigger());

                    builder.Register();

                }
                else
                {
                    foreach (var cur in BackgroundTaskRegistration.AllTasks)
                    {
                        if (cur.Value.Name == Constants.ToastBackgroundTask)
                        {
                            cur.Value.Unregister(true);
                        }
                    }
                }
                SettingHelper.UseActionCenterMode = value;
                OnPropertyChanged();
            }
        }

        private bool _userCacheCanDelete;
        public bool UserCacheCanDelete
        {
            get => _userCacheCanDelete;
            set => Set(ref _userCacheCanDelete, value);
        }

        private string _userCacheSize;
        public string UserCacheSize
        {
            get => _userCacheSize;
            set => Set(ref _userCacheSize, value);
        }

        private string _bangumiDataStatus = "检查更新";
        public string BangumiDataStatus
        {
            get => _bangumiDataStatus;
            set
            {
                Set(ref _bangumiDataStatus, value);
                OnPropertyChanged(nameof(BangumiDataVersion));
                OnPropertyChanged(nameof(LastUpdate));
            }
        }

        private bool _bangumiDataVersionChecking;
        public bool BangumiDataVersionChecking
        {
            get => _bangumiDataVersionChecking;
            set => Set(ref _bangumiDataVersionChecking, value);
        }

        public string BangumiDataVersion
        {
            get
            {
                // 显示当前数据版本及更新后版本
                var version = string.IsNullOrEmpty(BangumiData.Version) ?
                       "无数据" : BangumiData.Version;
                version += ((string.IsNullOrEmpty(BangumiData.LatestVersion) || BangumiData.Version == BangumiData.LatestVersion) ?
                       string.Empty :
                       $" -> {BangumiData.LatestVersion}");
                return version;
            }
        }

        public string LastUpdate
        {
            get
            {
                if (BangumiData.LastUpdate == DateTimeOffset.MinValue)
                {
                    return "从未更新";
                }
                return BangumiData.LastUpdate.ToString("yyyy-MM-dd");
            }
        }

        public async void Load()
        {
            if (UserCacheSize == "正在计算")
            {
                return;
            }
            UserCacheCanDelete = false;
            UserCacheSize = "正在计算";

            // 获取缓存文件大小
            UserCacheSize = FileSizeHelper.GetSizeString(BangumiApi.BgmCache.GetFileLength());
            UserCacheCanDelete = true;
        }

        public void DeleteUserCacheFile()
        {
            // 删除缓存文件
            BangumiApi.BgmCache.Delete();
            UserCacheSize = "无缓存";
            UserCacheCanDelete = false;
        }

        /// <summary>
        /// 检查更新并下载最新版本
        /// </summary>
        public async void UpdateBangumiData()
        {
            if (_versionCheckFailed)
            {
                // 跳过版本检查，直接下载数据
                BangumiDataStatus = "正在下载数据";
                BangumiDataVersionChecking = true;
                if (await BangumiData.DownloadLatestBangumiData(null, true))
                {
                    NotificationHelper.Notify("数据下载成功！");
                    _versionCheckFailed = false;
                }
                else
                {
                    NotificationHelper.Notify("数据下载失败，请重试或稍后再试！", NotifyType.Error);
                }
            }
            else
            {
                BangumiDataStatus = "正在检查更新";
                BangumiDataVersionChecking = true;
                bool hasNew = false;
                var startDownloadAction = new Action(() =>
                {
                    hasNew = true;
                    BangumiDataStatus = "正在下载数据";
                });
                if (await BangumiData.DownloadLatestBangumiData(startDownloadAction))
                {
                    if (hasNew)
                    {
                        NotificationHelper.Notify("数据下载成功！");
                    }
                    else
                    {
                        NotificationHelper.Notify("已是最新版本！");
                    }
                }
                else
                {
                    if (hasNew)
                    {
                        NotificationHelper.Notify("数据下载失败，请重试或稍后再试！", NotifyType.Error);
                    }
                    else
                    {
                        // 标记检查版本失败，可能为该 IP 地址调用 GitHub API 超过速率限制，
                        // 允许强制更新，此时版本无效
                        _versionCheckFailed = true;
                        NotificationHelper.Notify("获取最新版本失败！", NotifyType.Error);
                    }
                }
            }
            BangumiDataStatus = _versionCheckFailed ? "强制更新" : "检查更新";
            BangumiDataVersionChecking = false;
        }

        /// <summary>
        /// 打开站点设置面板
        /// </summary>
        public async void OpenSitesContentDialog()
        {
            var dg = new SitesContentDialog();
            await dg.ShowAsync();
        }

    }

    public class Url
    {
        public string Uri { get; set; }
        public string Desc { get; set; }
    }
}
