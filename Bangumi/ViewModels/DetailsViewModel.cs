using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Api.Models;
using Bangumi.Api.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Bangumi.ViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        public DetailsViewModel()
        {
            InitViewModel();
        }

        public ObservableCollection<Ep> eps { get; private set; } = new ObservableCollection<Ep>();

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

        private string _imageSource = "ms-appx:///Assets/resource/err_404.png";
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

        private int _air_weekday;
        public int AirWeekday
        {
            get => _air_weekday;
            set => Set(ref _air_weekday, value);
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
        public ObservableCollection<Crt> characters { get; private set; } = new ObservableCollection<Crt>();
        public ObservableCollection<Staff> staffs { get; private set; } = new ObservableCollection<Staff>();
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
        public ObservableCollection<Blog> blogs { get; private set; } = new ObservableCollection<Blog>();
        public ObservableCollection<Topic> topics { get; private set; } = new ObservableCollection<Topic>();

        // 收藏状态，评分，吐槽
        public int myRate;
        public string myComment;
        public bool myPrivacy;
        private string _collectionStatusText;
        public string CollectionStatusText
        {
            get => _collectionStatusText;
            set => Set(ref _collectionStatusText, value);
        }

        private string _collectionStatusIcon;
        public string CollectionStatusIcon
        {
            get => _collectionStatusIcon;
            set => Set(ref _collectionStatusIcon, value);
        }

        public void InitViewModel()
        {
            IsLoading = false;
            NameCn = "";
            AirDate = "";
            AirWeekday = 0;
            Summary = "";
            OthersRates.Clear();
            OthersCollection = "";
            Score = 0;
            // 更多资料用
            characters.Clear();
            staffs.Clear();
            Name = "";
            MoreInfo = "";
            MoreSummary = "";
            // 收藏状态，评分，吐槽
            CollectionStatusText = "收藏";
            CollectionStatusIcon = "\uE006";
            myRate = 0;
            myComment = "";
            myPrivacy = false;
            blogs.Clear();
            topics.Clear();
            eps.Clear();
        }


        /// <summary>
        /// 更新收藏状态、评分、吐槽
        /// </summary>
        public async void EditCollectionStatus()
        {
            if (!OAuthHelper.IsLogin)
                return;
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog()
            {
                rate = myRate,
                comment = myComment,
                privacy = myPrivacy,
                collectionStatus = CollectionStatusText
            };
            MainPage.rootPage.hasDialog = true;
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync())
            {
                MainPage.rootPage.hasDialog = false;
                IsUpdating = true;
                IsStatusLoaded = false;
                if (await BangumiFacade.UpdateCollectionStatusAsync(SubjectId,
                    BangumiConverters.GetCollectionStatusEnum(collectionEditContentDialog.collectionStatus), collectionEditContentDialog.comment,
                    collectionEditContentDialog.rate.ToString(), collectionEditContentDialog.privacy == true ? "1" : "0"))
                {
                    myRate = collectionEditContentDialog.rate;
                    myComment = collectionEditContentDialog.comment;
                    myPrivacy = collectionEditContentDialog.privacy;
                    CollectionStatusText = collectionEditContentDialog.collectionStatus;
                    SetCollectionButton();
                    // 若状态修改为看过，且设置启用，则批量修改章节状态为看过
                    if (collectionEditContentDialog.collectionStatus == CollectionStatusEnum.collect.GetValue() && SettingHelper.EpsBatch)
                    {
                        int epId = 0;
                        string epsId = string.Empty;
                        foreach (var episode in eps.Where(ep => ep.Status == "Air" || ep.Status == "Today" || ep.Status == "NA"))
                        {
                            epsId += episode.Id.ToString() + ",";
                        }
                        epsId = epsId.TrimEnd(',');
                        if (epsId != string.Empty && await BangumiFacade.UpdateProgressBatchAsync(epId, EpStatusEnum.watched, epsId))
                        {
                            foreach (var episode in eps.Where(ep => ep.Status == "Air" || ep.Status == "Today" || ep.Status == "NA"))
                            {
                                episode.Status = "看过";
                            }
                        }
                        else
                        {
                            MainPage.rootPage.ToastInAppNotification.Show("无章节需要更新", 1500);
                        }

                    }
                }
                IsStatusLoaded = true;
                IsUpdating = false;
            }
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
                    if (!string.IsNullOrEmpty(status.GetValue()))
                    {
                        ep.Status = status.GetValue();
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
                foreach (var episode in eps)
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
                    foreach (var episode in eps)
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
                    MainPage.rootPage.ToastInAppNotification.Show("无章节需要更新", 1500);
                }
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 设置收藏按钮的图标
        /// </summary>
        private void SetCollectionButton()
        {
            if (CollectionStatusText == "收藏")
            {
                CollectionStatusIcon = "\uE006";
            }
            else if (CollectionStatusText == "抛弃")
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
                MainPage.rootPage.RefreshAppBarButton.IsEnabled = false;
                // 获取条目信息
                var subject = await BangumiFacade.GetSubjectAsync(SubjectId);

                if (subject != null && subject.Id.ToString() == SubjectId)
                {
                    // 条目标题
                    NameCn = string.IsNullOrEmpty(subject.NameCn) ? subject.Name : subject.NameCn;
                    // 条目图片
                    ImageSource = subject.Images?.Common;
                    // 放送日期
                    AirDate = subject.AirDate;
                    AirWeekday = subject.AirWeekday;
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
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._10, score = 10 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._9, score = 9 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._8, score = 8 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._7, score = 7 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._6, score = 6 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._5, score = 5 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._4, score = 4 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._3, score = 3 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._2, score = 2 });
                        simpleRates.Add(new SimpleRate { count = subject.Rating.Count._1, score = 1 });
                        double maxCount = simpleRates.Max().count;
                        OthersRates.Clear();
                        foreach (var item in simpleRates)
                        {
                            item.ratio = (double)item.count * 100 / maxCount;
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

                    // 更多资料
                    Name = subject.Name;
                    MoreSummary = subject.Summary;
                    MoreInfo = "作品分类：" + ((SubjectTypeEnum)subject.Type).GetValue();
                    MoreInfo += subject.AirDate == "0000-00-00" ? "" : "\n放送开始：" + subject.AirDate;
                    MoreInfo += subject.AirWeekday == 0 ? "" : "\n放送星期：" + Converters.GetWeekday(subject.AirWeekday);
                    MoreInfo += subject.Eps == null ? "" : "\n话数：" + subject.Eps.Count;
                    // 角色
                    characters.Clear();
                    if (subject.Characters != null)
                    {
                        foreach (var crt in subject.Characters)
                        {
                            characters.Add(crt);
                        }
                    }
                    // 演职人员
                    staffs.Clear();
                    if (subject.Staff != null)
                    {
                        foreach (var staff in subject.Staff)
                        {
                            staffs.Add(staff);
                        }
                    }
                    // 评论
                    blogs.Clear();
                    if (subject.Blogs != null)
                    {
                        foreach (var blog in subject.Blogs)
                        {
                            blogs.Add(blog);
                        }
                    }
                    // 讨论版
                    topics.Clear();
                    if (subject.Topics != null)
                    {
                        foreach (var topic in subject.Topics)
                        {
                            topics.Add(topic);
                        }
                    }
                    // 显示章节
                    if (subject.Eps != null)
                    {
                        // 在无章节信息时添加
                        if (eps.Count == 0)
                        {
                            foreach (var ep in subject.Eps)
                            {
                                eps.Add(ep);
                            }
                        }
                        // 在有章节信息时覆盖
                        else
                        {
                            foreach (var ep in subject.Eps)
                            {
                                var oldEp = eps.Where(e => e.Id == ep.Id).FirstOrDefault();
                                oldEp.Name = ep.Name;
                                oldEp.NameCn = ep.NameCn;
                                oldEp.Url = ep.Url;
                                oldEp.Duration = ep.Duration;
                                oldEp.AirDate = ep.AirDate;
                                oldEp.Desc = ep.Desc;
                                oldEp.Comment = ep.Comment;
                            }
                        }
                    }
                    IsDetailLoading = false;

                    // 确认用户登录状态
                    if (OAuthHelper.IsLogin)
                    {
                        // 显示用户章节状态
                        Progress progress = await BangumiFacade.GetProgressesAsync(SubjectId);
                        if (progress != null && progress.SubjectId.ToString() == SubjectId)
                        {
                            foreach (var ep in eps) //用户观看状态
                            {
                                var prog = progress?.Eps?.Where(p => p.Id == ep.Id).FirstOrDefault();
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
                        // 获取收藏、评分和吐槽信息。
                        SubjectStatus2 collectionStatus = await BangumiFacade.GetCollectionStatusAsync(SubjectId);
                        if (collectionStatus?.Status != null)
                        {
                            CollectionStatusText = collectionStatus.Status.Name;
                            myRate = collectionStatus.Rating;
                            myComment = collectionStatus.Comment;
                            myPrivacy = collectionStatus.Private == "1" ? true : false;
                        }
                        SetCollectionButton();
                        IsStatusLoaded = true;
                    }
                }
            }
            catch (Exception e)
            {
                MainPage.rootPage.ErrorInAppNotification.Show("加载详情失败！\n" + e.Message.Replace("\r\n\r\n", "\r\n").TrimEnd('\n').TrimEnd('\r'), 3000);
                //var msgDialog = new Windows.UI.Popups.MessageDialog("加载详情失败！\n" + e.Message) { Title = "错误！" };
                //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                //await msgDialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
                IsDetailLoading = false;
                IsProgressLoading = false;
                IsStatusLoaded = true;
                MainPage.rootPage.RefreshAppBarButton.IsEnabled = true;
            }
        }

    }

    public class SimpleRate : IComparable<SimpleRate>
    {

        public int count { get; set; }
        public int score { get; set; }
        public double ratio { get; set; }

        public int CompareTo(SimpleRate other)
        {
            return count.CompareTo(other.count);
        }
    }
}
