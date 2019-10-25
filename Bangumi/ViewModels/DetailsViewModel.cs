﻿using Bangumi.Api;
using Bangumi.Api.Models;
using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Data;
using Bangumi.Facades;
using Bangumi.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bangumi.ViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        #region 属性
        public ObservableCollection<Ep> Eps { get; private set; } = new ObservableCollection<Ep>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
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

        private bool _isStatusLoaded;
        public bool IsStatusLoaded
        {
            get => _isStatusLoaded;
            set => Set(ref _isStatusLoaded, value);
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

        private SubjectTypeEnum SubjectType { get; set; }

        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set => Set(ref _imageSource, value);
        }

        private string _name_cn;
        public string NameCn
        {
            get => _name_cn;
            set => Set(ref _name_cn, value);
        }

        private string _air_date;
        public string AirDate
        {
            get => _air_date;
            set => Set(ref _air_date, value);
        }

        private string _air_time;
        public string AirTime
        {
            get => _air_time;
            set => Set(ref _air_time, value);
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

        public ObservableCollection<SimpleRate> OthersRates { get; private set; } = new ObservableCollection<SimpleRate>();

        private string _otherscollection;
        public string OthersCollection
        {
            get => _otherscollection;
            set => Set(ref _otherscollection, value);
        }



        // 更多资料用
        public ObservableCollection<Crt> Characters { get; private set; } = new ObservableCollection<Crt>();
        public ObservableCollection<Staff> Staffs { get; private set; } = new ObservableCollection<Staff>();
        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        private string _moreInfo;
        public string MoreInfo
        {
            get => _moreInfo;
            set => Set(ref _moreInfo, value);
        }
        private string _moreSummary;
        public string MoreSummary
        {
            get => _moreSummary;
            set => Set(ref _moreSummary, value);
        }

        // 评论和讨论版
        public ObservableCollection<Blog> Blogs { get; private set; } = new ObservableCollection<Blog>();
        public ObservableCollection<Topic> Topics { get; private set; } = new ObservableCollection<Topic>();

        // 收藏状态，评分，吐槽
        private int myRate;
        private string myComment;
        private bool myPrivacy;
        private CollectionStatusEnum _collectionStatus;
        public string CollectionStatusText
        {
            get => _collectionStatus.GetDescCn(SubjectType);
            set => Set(ref _collectionStatus, CollectionStatusEnumEx.FromValue(value));
        }

        private string _collectionStatusIcon;
        public string CollectionStatusIcon
        {
            get => _collectionStatusIcon;
            set => Set(ref _collectionStatusIcon, value);
        }
        #endregion

        public DetailsViewModel()
        {
            InitViewModel();
        }

        public void InitViewModel()
        {
            IsLoading = false;
            ImageSource = "";
            NameCn = "";
            AirDate = "";
            AirTime = "";
            Summary = "";
            OthersRates.Clear();
            OthersCollection = "";
            Score = 0;
            // 更多资料用
            Characters.Clear();
            Staffs.Clear();
            Name = "";
            MoreInfo = "";
            MoreSummary = "";
            // 收藏状态，评分，吐槽
            CollectionStatusText = "";
            CollectionStatusIcon = "\uE006";
            myRate = 0;
            myComment = "";
            myPrivacy = false;
            Blogs.Clear();
            Topics.Clear();
            Eps.Clear();
        }


        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        public async void EditCollectionStatus()
        {
            if (!BangumiApi.IsLogin)
                return;
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog()
            {
                Rate = myRate,
                Comment = myComment,
                Privacy = myPrivacy,
                CollectionStatus = _collectionStatus,
                SubjectType = this.SubjectType,
                Title = this.NameCn,
            };
            MainPage.RootPage.HasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync())
            {
                IsUpdating = true;
                IsStatusLoaded = false;
                if (await BangumiFacade.UpdateCollectionStatusAsync(SubjectId,
                    collectionEditContentDialog.CollectionStatus, collectionEditContentDialog.Comment,
                    collectionEditContentDialog.Rate.ToString(), collectionEditContentDialog.Privacy == true ? "1" : "0"))
                {
                    myRate = collectionEditContentDialog.Rate;
                    myComment = collectionEditContentDialog.Comment;
                    myPrivacy = collectionEditContentDialog.Privacy;
                    CollectionStatusText = collectionEditContentDialog.CollectionStatus.GetValue();
                    SetCollectionIcon();
                    // 若状态修改为看过，且设置启用，则批量修改章节状态为看过
                    if (_collectionStatus == CollectionStatusEnum.Collect && SettingHelper.EpsBatch)
                    {
                        int epId = 0;
                        string epsId = string.Empty;
                        foreach (var episode in Eps.Where(ep => ep.Status == "Air" || ep.Status == "Today" || ep.Status == "NA"))
                        {
                            epsId += episode.Id.ToString() + ",";
                        }
                        epsId = epsId.TrimEnd(',');
                        if (epsId != string.Empty && await BangumiFacade.UpdateProgressBatchAsync(epId, EpStatusEnum.watched, epsId))
                        {
                            foreach (var episode in Eps.Where(ep => ep.Status == "Air" || ep.Status == "Today" || ep.Status == "NA"))
                            {
                                episode.Status = "看过";
                            }
                        }
                        else
                        {
                            NotificationHelper.Notify("无章节需要更新");
                        }

                    }
                }
                IsStatusLoaded = true;
                IsUpdating = false;
            }
            MainPage.RootPage.HasDialog = false;
        }

        /// <summary>
        /// 更新章节状态
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status">状态</param>
        public async void UpdateEpStatus(Ep ep, EpStatusEnum status)
        {
            if (ep != null)
            {
                IsUpdating = true;
                if (await BangumiFacade.UpdateProgressAsync(ep.Id.ToString(), status))
                {
                    if (!string.IsNullOrEmpty(status.GetCnName()))
                    {
                        ep.Status = status.GetCnName();
                    }
                    else
                    {
                        if (DateTime.Parse(ep.AirDate) < DateTime.Now)
                            ep.Status = "Air";
                        else
                            ep.Status = "NA";
                    }
                }
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 批量更新章节状态为看过
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status">状态</param>
        public async void UpdateEpStatusBatch(Ep ep, EpStatusEnum status)
        {
            if (ep != null && status == EpStatusEnum.watched)
            {
                IsUpdating = true;
                string epsId = string.Empty;
                foreach (var episode in Eps)
                {
                    if (episode.Status == "Air" || episode.Status == "Today" || episode.Status == "NA")
                        epsId += episode.Id.ToString() + ",";
                    if (episode.Id == ep.Id)
                    {
                        break;
                    }
                }
                epsId = epsId.TrimEnd(',');
                if (epsId != string.Empty && await BangumiFacade.UpdateProgressBatchAsync(ep.Id, status, epsId))
                {
                    foreach (var episode in Eps)
                    {
                        if (episode.Status == "Air" || episode.Status == "Today" || episode.Status == "NA")
                            episode.Status = "看过";
                        if (episode.Id == ep.Id)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    NotificationHelper.Notify("无章节需要更新");
                }
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 设置收藏按钮的图标
        /// </summary>
        private void SetCollectionIcon()
        {
            if (CollectionStatusText == CollectionStatusEnum.No.GetValue())
            {
                CollectionStatusIcon = "\uE006";
            }
            else if (CollectionStatusText == CollectionStatusEnum.Dropped.GetValue())
            {
                CollectionStatusIcon = "\uE007";
            }
            else
            {
                CollectionStatusIcon = "\uE00B";
            }
        }

        /// <summary>
        /// 加载详情和章节，
        /// 用户进度，收藏状态。
        /// </summary>
        public async void LoadDetails()
        {
            if (IsLoading)
                return;
            try
            {
                IsLoading = true;
                IsDetailLoading = true;
                IsProgressLoading = true;
                IsStatusLoaded = false;
                MainPage.RootPage.RefreshButton.IsEnabled = false;

                // 获取缓存
                Subject subjectCache = null;
                Progress progressCache = null;
                SubjectStatus2 subjectStatusCache = null;
                if (BangumiApi.BangumiCache.Subjects.TryGetValue(SubjectId, out subjectCache))
                {
                    ProcessSubject(subjectCache);

                    // 检查用户登录状态
                    if (BangumiApi.IsLogin)
                    {
                        if (BangumiApi.BangumiCache.Progresses.TryGetValue(SubjectId, out progressCache))
                        {
                            ProcessProgress(subjectCache, progressCache);
                        }
                        if (BangumiApi.BangumiCache.SubjectStatus.TryGetValue(SubjectId, out subjectStatusCache))
                        {
                            ProcessCollectionStatus(subjectStatusCache);
                        }
                    }
                }

                // 检查用户登录状态
                Task<Progress> progress = null;
                Task<SubjectStatus2> subjectStatus = null;
                if (BangumiApi.IsLogin)
                {
                    progress = BangumiApi.GetProgressesAsync(SubjectId);
                    subjectStatus = BangumiApi.GetCollectionStatusAsync(SubjectId);
                }
                // 获取最新数据
                Subject subject = await BangumiApi.GetSubjectAsync(SubjectId);

                if (subject != null && subject.Id.ToString() == SubjectId)
                {
                    bool subjectChanged = false;
                    if (!subjectCache.EqualsExT(subject))
                    {
                        ProcessSubject(subject);
                        subjectChanged = true;
                    }

                    // 检查用户登录状态
                    if (BangumiApi.IsLogin)
                    {
                        if (subjectChanged || !progressCache.EqualsExT(await progress))
                        {
                            ProcessProgress(subject, await progress);
                        }
                        if (!subjectStatusCache.EqualsExT(await subjectStatus))
                        {
                            ProcessCollectionStatus(await subjectStatus);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                NotificationHelper.Notify("加载详情失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'),
                                          NotificationHelper.NotifyType.Error);
            }
            finally
            {
                IsLoading = false;
                IsDetailLoading = false;
                IsProgressLoading = false;
                IsStatusLoaded = true;
                MainPage.RootPage.RefreshButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 处理条目信息
        /// </summary>
        /// <param name="subject"></param>
        private void ProcessSubject(Subject subject)
        {
            try
            {
                // 条目标题
                NameCn = string.IsNullOrEmpty(subject.NameCn) ? subject.Name : subject.NameCn;
                // 条目图片
                ImageSource = subject.Images.Common;
                // 放送日期
                AirDate = subject.AirDate;
                AirTime = Converters.GetWeekday(subject.AirWeekday);
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
                    List<SimpleRate> simpleRates = new List<SimpleRate>();
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._10, Score = 10 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._9, Score = 9 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._8, Score = 8 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._7, Score = 7 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._6, Score = 6 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._5, Score = 5 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._4, Score = 4 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._3, Score = 3 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._2, Score = 2 });
                    simpleRates.Add(new SimpleRate { Count = subject.Rating.Count._1, Score = 1 });
                    double sumCount = simpleRates.Sum(s => s.Count);
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

                // 条目类别
                SubjectType = (SubjectTypeEnum)subject.Type;
                // 更多资料
                Name = subject.Name;
                MoreSummary = subject.Summary;
                MoreInfo = "作品分类：" + SubjectType.GetDescCn();
                MoreInfo += subject.AirDate == "0000-00-00" ? "" : "\n放送开始：" + subject.AirDate;
                MoreInfo += subject.AirWeekday == 0 ? "" : "\n放送星期：" + Converters.GetWeekday(subject.AirWeekday);
                MoreInfo += subject.Eps.Count == 0 ? "" : "\n话数：" + subject.Eps.Count;
                // 角色
                Characters.Clear();
                if (subject.Characters != null)
                {
                    foreach (var crt in subject.Characters)
                    {
                        Characters.Add(crt);
                    }
                }
                // 演职人员
                Staffs.Clear();
                if (subject.Staff != null)
                {
                    foreach (var staff in subject.Staff)
                    {
                        Staffs.Add(staff);
                    }
                }
                // 评论
                Blogs.Clear();
                if (subject.Blogs != null)
                {
                    foreach (var blog in subject.Blogs)
                    {
                        Blogs.Add(blog);
                    }
                }
                // 讨论版
                Topics.Clear();
                if (subject.Topics != null)
                {
                    foreach (var topic in subject.Topics)
                    {
                        Topics.Add(topic);
                    }
                }
                // 显示章节
                if (subject.Eps.Count > 0)
                {
                    // 在无章节信息时添加
                    if (Eps.Count == 0)
                    {
                        foreach (var ep in subject.Eps)
                        {
                            Ep newEp = new Ep
                            {
                                Id = ep.Id,
                                Url = ep.Url,
                                Type = ep.Type,
                                Sort = ep.Sort,
                                Name = ep.Name,
                                NameCn = ep.NameCn,
                                Duration = ep.Duration,
                                AirDate = ep.AirDate,
                                Comment = ep.Comment,
                                Desc = ep.Desc,
                                Status = ep.Status
                            };
                            Eps.Add(newEp);
                        }
                    }
                    // 在有章节信息时覆盖
                    else
                    {
                        foreach (var ep in subject.Eps)
                        {
                            var oldEp = Eps.Where(e => e.Id == ep.Id).FirstOrDefault();
                            oldEp.Url = ep.Url;
                            oldEp.Type = ep.Type;
                            oldEp.Sort = ep.Sort;
                            oldEp.Name = ep.Name;
                            oldEp.NameCn = ep.NameCn;
                            oldEp.Duration = ep.Duration;
                            oldEp.AirDate = ep.AirDate;
                            oldEp.Comment = ep.Comment;
                            oldEp.Desc = ep.Desc;
                            oldEp.Status = ep.Status;
                        }
                    }
                }
                IsDetailLoading = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 处理用户章节状态
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="progress"></param>
        private void ProcessProgress(Subject subject, Progress progress)
        {
            try
            {
                if (progress != null && progress.SubjectId.ToString() == SubjectId)
                {
                    foreach (var ep in Eps) //用户观看状态
                    {
                        var prog = progress.Eps?.Where(p => p.Id == ep.Id).FirstOrDefault();
                        if (prog != null)
                        {
                            ep.Status = prog.Status.CnName;
                        }
                        else
                        {
                            ep.Status = subject.Eps.Where(e => e.Id == ep.Id).FirstOrDefault().Status;
                        }
                    }
                    IsProgressLoading = false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 处理收藏、评分和吐槽信息。
        /// </summary>
        /// <param name="subjectStatus"></param>
        private void ProcessCollectionStatus(SubjectStatus2 subjectStatus)
        {
            try
            {
                if (subjectStatus?.Status != null)
                {
                    CollectionStatusText = subjectStatus.Status.Type;
                    myRate = subjectStatus.Rating;
                    myComment = subjectStatus.Comment;
                    myPrivacy = subjectStatus.Private == "1" ? true : false;
                }
                else
                {
                    CollectionStatusText = "收藏";
                    myRate = 0;
                    myComment = string.Empty;
                    myPrivacy = false;
                }
                SetCollectionIcon();
                IsStatusLoaded = true;
            }
            catch (Exception)
            {

                throw;
            }
        }


    }

    public class SimpleRate : IComparable<SimpleRate>
    {

        public int Count { get; set; }
        public int Score { get; set; }
        public double Ratio { get; set; }

        public int CompareTo(SimpleRate other)
        {
            return Count.CompareTo(other.Count);
        }
    }
}
