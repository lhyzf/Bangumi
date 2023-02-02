using Bangumi.Api;
using Bangumi.Api.Common;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Controls;
using Bangumi.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bangumi.ViewModels
{
    public class EpisodeViewModel : ViewModelBase
    {
        #region 属性
        public ObservableCollection<GroupedEpisode> GroupedEps { get; private set; } = new ObservableCollection<GroupedEpisode>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                Set(ref _isLoading, value);
                MainPage.RootPage.PageStatusChanged();
            }
        }

        private bool _isDetailLoading;
        public bool IsDetailLoading
        {
            get => _isDetailLoading;
            set => Set(ref _isDetailLoading, value);
        }

        private bool _isProgressLoading;
        public bool IsProgressLoading
        {
            get => _isProgressLoading;
            set => Set(ref _isProgressLoading, value);
        }

        private bool _isStatusLoading;
        public bool IsStatusLoading
        {
            get => _isStatusLoading;
            set => Set(ref _isStatusLoading, value);
        }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set => Set(ref _isUpdating, value);
        }

        private string _subjectId;
        public string SubjectId
        {
            get => _subjectId;
            set => Set(ref _subjectId, value);
        }

        private SubjectType _subjectType;
        public SubjectType SubjectType
        {
            get => _subjectType;
            set => Set(ref _subjectType, value);
        }

        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set => Set(ref _imageSource, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _airDate;
        public string AirDate
        {
            get => _airDate;
            set => Set(ref _airDate, value);
        }

        private string _airTime;
        public string AirTime
        {
            get => _airTime;
            set => Set(ref _airTime, value);
        }

        private string _summary;
        public string Summary
        {
            get => _summary;
            set => Set(ref _summary, value);
        }

        private double _score;
        public double Score
        {
            get => _score;
            set => Set(ref _score, value);
        }

        private string _ratingCount;
        public string RatingCount
        {
            get => _ratingCount;
            set => Set(ref _ratingCount, value);
        }

        public ObservableCollection<SimpleRate> OthersRates { get; private set; } = new ObservableCollection<SimpleRate>();

        private string _othersCollection;
        public string OthersCollection
        {
            get => _othersCollection;
            set => Set(ref _othersCollection, value);
        }

        // 书籍类型的条目使用
        private int? _chapCount;
        /// <summary>
        /// 章节数
        /// </summary>
        public int? ChapCount
        {
            get => _chapCount;
            set
            {
                if (Set(ref _chapCount, value))
                {
                    OnPropertyChanged(nameof(ChapCountString));
                }
            }
        }
        /// <inheritdoc cref="ChapCount"/>
        public string ChapCountString
        {
            get => ChapCount?.ToString() ?? "??";
        }

        private int? _volCount;
        /// <summary>
        /// 单行本数
        /// </summary>
        public int? VolCount
        {
            get => _volCount;
            set
            {
                if (Set(ref _volCount, value))
                {
                    OnPropertyChanged(nameof(VolCountString));
                }
            }
        }
        /// <inheritdoc cref="VolCount"/>
        public string VolCountString
        {
            get => VolCount?.ToString() ?? "??";
        }

        private int _chapStatus;
        /// <summary>
        /// 章节状态
        /// </summary>
        public int ChapStatus
        {
            get => _chapStatus;
            set
            {
                if (value >= 0)
                {
                    Set(ref _chapStatus, value);
                }
                else
                {
                    OnPropertyChanged();
                }
            }
        }

        private int _volStatus;
        /// <summary>
        /// 单行本状态
        /// </summary>
        public int VolStatus
        {
            get => _volStatus;
            set
            {
                if (value >= 0)
                {
                    Set(ref _volStatus, value);
                }
                else
                {
                    OnPropertyChanged();
                }
            }
        }

        // 详情
        public DetailViewModel Detail { get; set; }

        // 收藏状态，评分，吐槽
        private CollectionStatusType? _collectionStatus;
        private void SetCollectionStatus(CollectionStatusType? value)
        {
            Set(ref _collectionStatus, value);
            OnPropertyChanged(nameof(CollectionStatusText));
            OnPropertyChanged(nameof(CollectionStatusIcon));
        }

        public string CollectionStatusText => _collectionStatus?.GetDesc(SubjectType) ?? "收藏";

        public string CollectionStatusIcon => _collectionStatus switch
        {
            CollectionStatusType.Dropped => "\uE007",
            null => "\uE006",
            _ => "\uE00B",
        };
        #endregion


        public void InitViewModel()
        {
            IsLoading = false;
            ImageSource = string.Empty;
            Name = string.Empty;
            AirDate = string.Empty;
            AirTime = string.Empty;
            Summary = string.Empty;
            OthersRates.Clear();
            OthersCollection = string.Empty;
            Score = 0;
            RatingCount = string.Empty;
            // 收藏状态，评分，吐槽
            SetCollectionStatus(null);
            GroupedEps.Clear();
        }


        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        public async void EditCollectionStatus()
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (!BangumiApi.BgmOAuth.IsLogin)
            {
                NotificationHelper.Notify("请先登录！", NotifyType.Warn);
                return;
            }
            var subjectStatus = BangumiApi.BgmApi.Status(SubjectId);
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog(
                this.SubjectType, subjectStatus)
            {
                Title = this.Name,
            };
            MainPage.RootPage.HasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync() &&
                collectionEditContentDialog.CollectionStatus != null)
            {
                IsUpdating = true;
                IsStatusLoading = true;
                // 若状态修改为看过，且设置启用，则批量修改正片章节状态为看过
                if (collectionEditContentDialog.CollectionStatus.Value == CollectionStatusType.Collect && SettingHelper.EpsBatch)
                {
                    var selectedEps = GroupedEps.SelectMany(g => g.Where(ep => ep.Type == EpisodeType.本篇 && ep.EpStatus == EpStatusType.remove));
                    int epId = selectedEps.LastOrDefault()?.Id ?? 0;
                    string epsId = string.Join(',', selectedEps.Select(it => it.Id));
                    if (!string.IsNullOrEmpty(epsId))
                    {
                        try
                        {
                            if (await BangumiApi.BgmApi.UpdateProgressBatch(epId, epsId))
                            {
                                foreach (var episode in selectedEps)
                                {
                                    episode.EpStatus = EpStatusType.watched;
                                }
                                NotificationHelper.Notify("批量标记章节看过成功！");
                            }
                        }
                        catch (Exception e)
                        {
                            NotificationHelper.Notify("批量标记章节看过失败！\n错误信息：" + e.Message,
                                                      NotifyType.Error);
                        }
                    }
                    else
                    {
                        NotificationHelper.Notify("无章节需要标记！");
                    }
                }
                try
                {
                    var collectionStatusE = await BangumiApi.BgmApi.UpdateStatus(SubjectId,
                       collectionEditContentDialog.CollectionStatus.Value,
                       collectionEditContentDialog.Comment,
                       collectionEditContentDialog.Rate.ToString(),
                       collectionEditContentDialog.Privacy ? "1" : "0");
                    SetCollectionStatus(collectionStatusE.Status.Id);
                    NotificationHelper.Notify($"标记 {collectionEditContentDialog.Title} {collectionStatusE.Status.Id.GetDesc(this.SubjectType)} 成功！");
                }
                catch (Exception e)
                {
                    NotificationHelper.Notify($"标记 {collectionEditContentDialog.Title} {collectionEditContentDialog.CollectionStatus?.GetDesc(this.SubjectType)} 失败！\n" + e.Message,
                                              NotifyType.Error);
                }
                IsStatusLoading = false;
                IsUpdating = false;
            }
            MainPage.RootPage.HasDialog = false;
        }

        /// <summary>
        /// 更新章节状态
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status">状态</param>
        public async Task UpdateEpStatus(EpisodeWithEpStatus ep, EpStatusType status)
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (ep != null)
            {
                try
                {
                    IsUpdating = true;
                    if (await BangumiApi.BgmApi.UpdateProgress(ep.Id.ToString(), status))
                    {
                        ep.EpStatus = status;
                        NotificationHelper.Notify($"标记 ep.{ep.Sort} {Converters.StringOneOrTwo(ep.NameCn, ep.Name)} {status.GetCnName()}成功");
                    }
                    else
                    {
                        NotificationHelper.Notify($"标记 ep.{ep.Sort} {Converters.StringOneOrTwo(ep.NameCn, ep.Name)} {status.GetCnName()}失败，请重试！",
                                                  NotifyType.Warn);
                    }
                }
                catch (Exception e)
                {
                    NotificationHelper.Notify($"标记 ep.{ep.Sort} {Converters.StringOneOrTwo(ep.NameCn, ep.Name)} {status.GetCnName()}失败！\n错误信息：{e.Message}",
                                              NotifyType.Error);
                }
                finally
                {
                    IsUpdating = false;
                }
            }
        }

        /// <summary>
        /// 批量更新章节状态为看过
        /// </summary>
        /// <param name="ep"></param>
        public async Task UpdateEpStatusBatch(EpisodeWithEpStatus ep)
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            if (ep == null)
            {
                return;
            }
            IsUpdating = true;
            var selectedEps = GetSelectedEps();
            string epsId = string.Join(',', selectedEps.Select(it => it.Id.ToString()));
            if (!string.IsNullOrEmpty(epsId))
            {
                try
                {
                    if (await BangumiApi.BgmApi.UpdateProgressBatch(ep.Id, epsId))
                    {
                        foreach (var episode in selectedEps)
                        {
                            episode.EpStatus = EpStatusType.watched;
                        }
                        NotificationHelper.Notify("批量标记章节看过成功！");
                    }
                }
                catch (Exception e)
                {
                    NotificationHelper.Notify("批量标记章节看过失败！\n错误信息：" + e.Message,
                                              NotifyType.Error);
                }
            }
            else
            {
                NotificationHelper.Notify("无章节需要标记！");
            }
            IsUpdating = false;

            // 获取点击看到章节及之前所有未标记的章节
            IEnumerable<EpisodeWithEpStatus> GetSelectedEps()
            {
                foreach (var episode in GroupedEps.SelectMany(g => g.Where(e => e.Type == ep.Type)))
                {
                    if (episode.EpStatus == EpStatusType.remove)
                    {
                        yield return episode;
                    }
                    if (episode.Id == ep.Id)
                    {
                        yield break;
                    }
                }
            }
        }

        /// <summary>
        /// 更新书籍阅读状态
        /// </summary>
        public async Task UpdateBookStatus()
        {
            if (NetworkHelper.IsOffline)
            {
                NotificationHelper.Notify("无网络连接！", NotifyType.Warn);
                return;
            }
            try
            {
                IsUpdating = true;
                if (await BangumiApi.BgmApi.UpdateBookProgress(SubjectId, ChapStatus.ToString(), VolStatus.ToString()))
                {
                    NotificationHelper.Notify($"标记 Chap.{ChapStatus} Vol.{VolStatus} 成功");
                }
                else
                {
                    NotificationHelper.Notify($"标记 Chap.{ChapStatus} Vol.{VolStatus} 失败，请重试！",
                                              NotifyType.Warn);
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify($"标记 Chap.{ChapStatus} Vol.{VolStatus} 失败！\n错误信息：{e.Message}",
                                          NotifyType.Error);
            }
            finally
            {
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 加载详情和章节，
        /// 用户进度，收藏状态。
        /// </summary>
        public async Task LoadDetails()
        {
            if (NetworkHelper.IsOffline)
            {
                LoadDetailsFromCache();
                return;
            }
            if (IsLoading)
            {
                return;
            }
            try
            {
                IsLoading = true;
                IsDetailLoading = true;
                IsProgressLoading = true;
                IsStatusLoading = true;

                var subjectTask = BangumiApi.BgmApi.Subject(SubjectId);
                var cachedSubject = BangumiApi.BgmCache.Subject(SubjectId);
                ProcessSubject(cachedSubject);
                // 检查用户登录状态
                if (BangumiApi.BgmOAuth.IsLogin)
                {
                    var progress = BangumiApi.BgmApi.Progress(SubjectId);
                    var status = BangumiApi.BgmApi.Status(SubjectId);
                    if (cachedSubject != null &&
                        (cachedSubject.Type == SubjectType.Anime ||
                        cachedSubject.Type == SubjectType.Real))
                    {
                        ProcessProgress(BangumiApi.BgmCache.Progress(SubjectId));
                    }
                    ProcessCollectionStatus(BangumiApi.BgmCache.Status(SubjectId));
                    var subject = await subjectTask;
                    ProcessSubject(subject);
                    IsDetailLoading = false;
                    // 动画、三次元有独立的章节状态
                    if (subject != null &&
                        (subject.Type == SubjectType.Anime ||
                        subject.Type == SubjectType.Real))
                    {
                        ProcessProgress(await progress);
                    }
                    IsProgressLoading = false;
                    ProcessCollectionStatus(await status);
                    IsStatusLoading = false;
                }
                else
                {
                    ProcessSubject(await subjectTask);
                    IsDetailLoading = false;
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("加载详情失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
                IsDetailLoading = false;
                IsProgressLoading = false;
                IsStatusLoading = false;
            }
        }

        /// <summary>
        /// 从缓存加载详情和章节，
        /// 用户进度，收藏状态。
        /// </summary>
        public void LoadDetailsFromCache()
        {
            var subject = BangumiApi.BgmCache.Subject(SubjectId);
            ProcessSubject(subject);
            if (subject != null &&
                (subject.Type == SubjectType.Anime ||
                subject.Type == SubjectType.Real))
            {
                ProcessProgress(BangumiApi.BgmCache.Progress(SubjectId));
            }
            ProcessCollectionStatus(BangumiApi.BgmCache.Status(SubjectId));
        }

        /// <summary>
        /// 处理条目信息
        /// </summary>
        /// <param name="subject"></param>
        private void ProcessSubject(SubjectLarge subject)
        {
            if (subject == null || subject.Id.ToString() != SubjectId)
            {
                return;
            }
            // 条目标题
            Name = Converters.StringOneOrTwo(subject.NameCn, subject.Name);
            // 条目图片
            ImageSource = subject.Images?.Common;
            // 放送日期
            AirDate = subject.AirDate;
            AirTime = subject.AirWeekdayCn;
            // 条目简介
            if (!string.IsNullOrEmpty(subject.Summary))
            {
                Summary = subject.Summary.Length > 80 ? (subject.Summary.Substring(0, 80) + "...") : subject.Summary;
            }
            else
            {
                Summary = "暂无简介";
            }
            if (subject.Rating != null)
            {
                Score = subject.Rating.Score;
                var simpleRates = new List<SimpleRate>
                {
                    new SimpleRate {Count = subject.Rating.Count._10, Score = 10},
                    new SimpleRate {Count = subject.Rating.Count._9, Score = 9},
                    new SimpleRate {Count = subject.Rating.Count._8, Score = 8},
                    new SimpleRate {Count = subject.Rating.Count._7, Score = 7},
                    new SimpleRate {Count = subject.Rating.Count._6, Score = 6},
                    new SimpleRate {Count = subject.Rating.Count._5, Score = 5},
                    new SimpleRate {Count = subject.Rating.Count._4, Score = 4},
                    new SimpleRate {Count = subject.Rating.Count._3, Score = 3},
                    new SimpleRate {Count = subject.Rating.Count._2, Score = 2},
                    new SimpleRate {Count = subject.Rating.Count._1, Score = 1}
                };
                int sumCount = simpleRates.Sum(s => s.Count);
                RatingCount = "x" + sumCount.ToString();
                OthersRates.Clear();
                foreach (var item in simpleRates)
                {
                    item.Ratio = (double)item.Count * 100 / sumCount;
                    OthersRates.Add(item);
                }
            }
            if (subject.Collection != null)
            {
                OthersCollection = subject.Collection.Wish + "想看/" +
                                   subject.Collection.Collect + "看过/" +
                                   subject.Collection.Doing + "在看/" +
                                   subject.Collection.OnHold + "搁置/" +
                                   subject.Collection.Dropped + "抛弃";
            }

            SubjectType = subject.Type;
            // 详情
            var infoList = new List<(string, string)>();
            if (!string.IsNullOrEmpty(subject.NameCn))
            {
                infoList.Add(("中文名", subject.NameCn));
            }
            infoList.Add(("作品分类", subject.Type.GetDesc()));
            if (subject.Type == SubjectType.Anime || subject.Type == SubjectType.Real)
            {
                if (subject.AirDate != "0000-00-00")
                {
                    infoList.Add(("放送开始", subject.AirDate));
                }
                if (subject.AirWeekday != 0)
                {
                    infoList.Add(("放送星期", subject.AirWeekdayCn));
                }
                if (subject.Eps != null)
                {
                    infoList.Add(("话数", subject.Eps.Count.ToString()));
                }
            }
            else if (subject.Type == SubjectType.Book)
            {
                infoList.Add(("发售日", subject.AirDate));
            }
            else if (subject.Type == SubjectType.Music)
            {
                infoList.Add(("发售日期", subject.AirDate));
            }
            else if (subject.Type == SubjectType.Game)
            {
                infoList.Add(("发行日期", subject.AirDate));
            }
            string info = string.Join(Environment.NewLine, infoList.Select(it => it.Item1 + "：" + it.Item2));
            Detail = new DetailViewModel
            {
                Name = subject.Name,
                NameCn = subject.NameCn,
                Summary = subject.Summary,
                Info = info,
                Characters = subject.Characters,
                Staffs = subject.Staff,
                Blogs = subject.Blogs,
                Topics = subject.Topics
            };

            // 章节
            if (subject.Eps != null)
            {
                var eps = new List<EpisodeWithEpStatus>();
                foreach (var ep in subject.Eps)
                {
                    eps.Add(EpisodeWithEpStatus.FromEpisode(ep));
                }
                var groupEps = eps.GroupBy(ep => ep.Type)
                    .OrderBy(g => g.Key)
                    .Select(g => new GroupedEpisode(g) { Key = g.Key.GetDesc() });
                if (!groupEps.SequenceEqualExT(GroupedEps))
                {
                    GroupedEps.Clear();
                    foreach (var item in groupEps)
                    {
                        GroupedEps.Add(item);
                    }
                }
            }
            // 书籍信息
            ChapCount = subject.EpsCount;
            VolCount = subject.VolsCount;
        }

        private void ProcessCollectionStatus(CollectionStatusE collectionStatusE)
        {
            SetCollectionStatus(collectionStatusE?.Status?.Id);
            if (collectionStatusE != null)
            {
                ChapStatus = collectionStatusE.EpStatus;
                VolStatus = collectionStatusE.VolStatus;
            }
        }

        /// <summary>
        /// 处理用户章节状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="progress"></param>
        private void ProcessProgress(Progress progress)
        {
            if (progress?.Eps == null || progress.SubjectId.ToString() != SubjectId)
            {
                return;
            }
            foreach (var group in GroupedEps) //用户观看状态
            {
                foreach (EpisodeWithEpStatus ep in group)
                {
                    var prog = progress.Eps?.Where(p => p.Id == ep.Id).FirstOrDefault();
                    ep.EpStatus = prog?.Status?.Id ?? EpStatusType.remove;
                }
            }
        }
    }

    public class GroupedEpisode : List<EpisodeWithEpStatus>
    {
        public GroupedEpisode(IEnumerable<EpisodeWithEpStatus> items) : base(items)
        {
        }
        public string Key { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var g = (GroupedEpisode)obj;
            return Key == g.Key &&
                   this.SequenceEqualExT(g);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class SimpleRate
    {
        public int Count { get; set; }
        public int Score { get; set; }
        public double Ratio { get; set; }
    }
}
