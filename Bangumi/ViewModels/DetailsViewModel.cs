using Bangumi.Common;
using Bangumi.ContentDialogs;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
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

        public string NoImageUri;
        public ObservableCollection<Ep> eps { get; private set; } = new ObservableCollection<Ep>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private bool _isCollectionStatusLoading;
        public bool IsCollectionStatusLoading
        {
            get => _isCollectionStatusLoading;
            set => Set(ref _isCollectionStatusLoading, value);
        }

        private bool _isRateLoading;
        public bool IsRateLoading
        {
            get => _isRateLoading;
            set => Set(ref _isRateLoading, value);
        }

        private bool _isDetailLoading;
        public bool IsDetailLoading
        {
            get => _isDetailLoading;
            set => Set(ref _isDetailLoading, value);
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

        private string _imageSource = "";
        public string ImageSource
        {
            get => _imageSource;
            set => Set(ref _imageSource, value);
        }

        private string _name_cn;
        public string Name_cn
        {
            get => _name_cn;
            set => Set(ref _name_cn, value);
        }

        private string _air_date;
        public string Air_date
        {
            get => _air_date;
            set => Set(ref _air_date, value);
        }

        private int _air_weekday;
        public int Air_weekday
        {
            get => _air_weekday;
            set => Set(ref _air_weekday, value);
        }

        private string _airWeekdayName;
        public string AirWeekdayName
        {
            get => _airWeekdayName;
            set => Set(ref _airWeekdayName, value);
        }

        private string _summary;
        public string Summary
        {
            get => _summary;
            set => Set(ref _summary, value);
        }

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

        // 更多资料用
        public string name;
        public string moreInfo;
        public string moreSummary;
        public string moreCharacters;
        public string moreStaff;

        // 评分、吐槽用
        public int myRate;
        public string myComment;
        public bool myPrivacy;

        public void InitViewModel()
        {
            IsLoading = true;
            IsCollectionStatusLoading = true;
            IsDetailLoading = true;
            IsRateLoading = true;
            NoImageUri = Constants.noImageUri;
            Name_cn = "";
            Air_date = "";
            Air_weekday = 0;
            AirWeekdayName = "";
            Summary = "";
            CollectionStatusText = "收藏";
            CollectionStatusIcon = "\uE006";
            eps.Clear();
        }

        public async void EditMyRate()
        {
            CollectionEditContentDialog collectionEditContentDialog = new CollectionEditContentDialog()
            {
                rate = myRate,
                comment = myComment,
                privacy = myPrivacy,
            };
            if (ContentDialogResult.Primary == await collectionEditContentDialog.ShowAsync())
            {
                IsUpdating = true;
                IsRateLoading = true;
                if (await BangumiFacade.UpdateCollectionStatusAsync(SubjectId, GetStatusEnum(), collectionEditContentDialog.comment,
                     collectionEditContentDialog.rate.ToString(), collectionEditContentDialog.privacy == true ? "1" : "0"))
                {
                    LoadCollectionStatus();
                }
                IsUpdating = false;
            }

        }

        /// <summary>
        /// 更新章节状态
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status">状态</param>
        public async void UpdateEpStatus(Ep ep, BangumiFacade.EpStatusEnum status)
        {
            if (ep != null)
            {
                IsUpdating = true;
                if (await BangumiFacade.UpdateProgressAsync(ep.id.ToString(), status))
                {
                    switch (status)
                    {
                        case BangumiFacade.EpStatusEnum.watched:
                            ep.status = "看过";
                            break;
                        case BangumiFacade.EpStatusEnum.queue:
                            ep.status = "想看";
                            break;
                        case BangumiFacade.EpStatusEnum.drop:
                            ep.status = "抛弃";
                            break;
                        case BangumiFacade.EpStatusEnum.remove:
                            ep.status = "";
                            break;
                        default:
                            break;
                    }
                }
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 批量更新章节状态
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="status">状态</param>
        public async void UpdateEpStatusBatch(Ep ep, BangumiFacade.EpStatusEnum status)
        {
            if (ep != null && status == BangumiFacade.EpStatusEnum.watched)
            {
                IsUpdating = true;
                string epsId = string.Empty;
                foreach (var episode in eps)
                {
                    if (episode.id == ep.id)
                    {
                        epsId += episode.id.ToString();
                        break;
                    }
                    else
                    {
                        epsId += episode.id.ToString() + ",";
                    }
                }
                if (await BangumiFacade.UpdateProgressBatchAsync(ep.id, status, epsId))
                {
                    foreach (var episode in eps)
                    {
                        episode.status = "看过";
                        if (episode.id == ep.id)
                        {
                            break;
                        }
                    }
                }
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 更新收藏状态
        /// </summary>
        /// <param name="status">状态</param>
        public async void UpdateCollectionStatus(BangumiFacade.CollectionStatusEnum status)
        {
            IsUpdating = true;
            if (await BangumiFacade.UpdateCollectionStatusAsync(SubjectId, status))
            {
                if (status == BangumiFacade.CollectionStatusEnum.collect && SettingHelper.EpsBatch == true)
                {
                    int epId = 0;
                    string epsId = string.Empty;
                    foreach (var episode in eps)
                    {
                        if (eps.IndexOf(episode) == eps.Count - 1)
                        {
                            epsId += episode.id.ToString();
                            epId = episode.id;
                            break;
                        }
                        else
                        {
                            epsId += episode.id.ToString() + ",";
                        }
                    }
                    if (await BangumiFacade.UpdateProgressBatchAsync(epId, BangumiFacade.EpStatusEnum.watched, epsId))
                    {
                        foreach (var episode in eps)
                        {
                            episode.status = "看过";
                        }
                    }
                }
                CollectionStatusText = GetStatusName(status);
                SetCollectionButton();
            }
            IsUpdating = false;
        }

        /// <summary>
        /// 设置收藏按钮
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
        /// 获取收藏、评分和吐槽信息。
        /// </summary>
        public async void LoadCollectionStatus()
        {
            if (!OAuthHelper.IsLogin)
            {
                return;
            }
            try
            {
                CollectionStatus collectionStatus = await BangumiFacade.GetCollectionStatusAsync(SubjectId);
                if (collectionStatus != null && collectionStatus.status != null)
                {
                    CollectionStatusText = collectionStatus.status.name;
                    myRate = collectionStatus.rating;
                    myComment = collectionStatus.comment;
                    myPrivacy = collectionStatus.@private == "1" ? true : false;
                }
                SetCollectionButton();
                IsCollectionStatusLoading = false;
                IsRateLoading = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 加载详情和章节。
        /// </summary>
        public async void LoadDetails()
        {
            try
            {
                IsLoading = true;
                IsDetailLoading = true;
                // 获取条目信息
                var subject = await BangumiFacade.GetSubjectAsync(SubjectId);

                if (subject != null)
                {
                    // 条目标题
                    if (string.IsNullOrEmpty(Name_cn))
                    {
                        Name_cn = string.IsNullOrEmpty(subject.name_cn) ? subject.name : subject.name_cn;
                    }
                    // 条目图片
                    if (ImageSource.Equals(NoImageUri) && subject.images != null)
                    {
                        ImageSource = subject.images.common;
                    }
                    // 放送日期
                    if (string.IsNullOrEmpty(Air_date))
                    {
                        Air_date = subject.air_date;
                        Air_weekday = subject.air_weekday;
                        AirWeekdayName = GetWeekday(subject.air_weekday);
                    }
                    // 条目简介
                    if (!string.IsNullOrEmpty(subject.summary))
                    {
                        Summary = subject.summary.Length > 80 ? (subject.summary.Substring(0, 80) + "...") : subject.summary;
                    }
                    else
                    {
                        Summary = "暂无简介";
                    }

                    // 更多资料
                    name = subject.name;
                    moreSummary = string.IsNullOrEmpty(subject.summary) ? "暂无简介" : subject.summary;
                    moreInfo = "作品分类：" + GetSubjectTypeCn((BangumiFacade.SubjectType)subject.type);
                    moreInfo += "\n放送开始：" + subject.air_date;
                    moreInfo += "\n放送星期：" + GetWeekday(subject.air_weekday);
                    moreInfo += "\n话数：" + subject.eps_count;
                    // 角色
                    if (subject.crt != null)
                    {
                        foreach (var crt in subject.crt)
                        {
                            moreCharacters += string.Format("{0}：", string.IsNullOrEmpty(crt.name_cn) ? crt.name : crt.name_cn);
                            if (crt.actors != null)
                            {
                                foreach (var actor in crt.actors)
                                {
                                    moreCharacters += actor.name + "、";
                                }
                                moreCharacters = moreCharacters.TrimEnd('、');
                            }
                            else
                            {
                                moreCharacters += "暂无资料";
                            }
                            moreCharacters += "\n";
                        }
                        moreCharacters = moreCharacters.TrimEnd('\n');
                    }
                    else
                    {
                        moreCharacters += "暂无资料";
                    }
                    // 演职人员
                    if (subject.staff != null)
                    {
                        var sd = new Dictionary<string, string>();
                        foreach (var staff in subject.staff)
                        {
                            foreach (var job in staff.jobs)
                            {
                                if (!sd.ContainsKey(job))
                                {
                                    sd.Add(job, string.IsNullOrEmpty(staff.name_cn) ? staff.name : staff.name_cn);
                                }
                                else
                                {
                                    sd[job] += string.Format("、{0}", string.IsNullOrEmpty(staff.name_cn) ? staff.name : staff.name_cn);
                                }
                            }
                        }
                        foreach (var s in sd)
                        {
                            moreStaff += string.Format("{0}：{1}\n", s.Key, s.Value);
                        }
                        moreStaff = moreStaff.TrimEnd('\n');
                    }
                    else
                    {
                        moreStaff += "暂无资料";
                    }
                    // 显示章节
                    if (subject.eps != null)
                    {
                        if (eps.Count == 0)
                        {
                            foreach (var ep in subject.eps.OrderBy(c => c.type))
                            {
                                if (ep.type == 0)
                                {
                                    ep.sort = "第 " + ep.sort + " 话";
                                }
                                else
                                {
                                    ep.sort = GetEpisodeType(ep.type) + " " + ep.sort;
                                }
                                eps.Add(ep);
                            }
                        }
                    }
                    IsDetailLoading = false;

                    // 显示用户章节状态
                    if (OAuthHelper.IsLogin)
                    {
                        Progress progress = await BangumiFacade.GetProgressesAsync(SubjectId);
                        if (progress != null)
                        {
                            foreach (var ep in eps) //用户观看状态
                            {
                                foreach (var p in progress.eps)
                                {
                                    if (p.id == ep.id)
                                    {
                                        ep.status = p.status.cn_name;
                                        progress.eps.Remove(p);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    IsLoading = false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }


        private string GetSubjectTypeCn(BangumiFacade.SubjectType type)
        {
            string cn = "";
            switch (type)
            {
                case BangumiFacade.SubjectType.book:
                    cn = "书籍";
                    break;
                case BangumiFacade.SubjectType.anime:
                    cn = "动画";
                    break;
                case BangumiFacade.SubjectType.music:
                    cn = "音乐";
                    break;
                case BangumiFacade.SubjectType.game:
                    cn = "游戏";
                    break;
                case BangumiFacade.SubjectType.real:
                    cn = "三次元";
                    break;
                default:
                    break;
            }
            return cn;
        }

        public string GetWeekday(int day)
        {
            var weekday = "";
            switch (day)
            {
                case 1:
                    weekday = "星期一";
                    break;
                case 2:
                    weekday = "星期二";
                    break;
                case 3:
                    weekday = "星期三";
                    break;
                case 4:
                    weekday = "星期四";
                    break;
                case 5:
                    weekday = "星期五";
                    break;
                case 6:
                    weekday = "星期六";
                    break;
                case 7:
                    weekday = "星期日";
                    break;
                default:
                    break;
            }
            return weekday;
        }

        private string GetStatusName(BangumiFacade.CollectionStatusEnum status)
        {
            string result;
            switch (status)
            {
                case BangumiFacade.CollectionStatusEnum.wish:
                    result = "想看";
                    break;
                case BangumiFacade.CollectionStatusEnum.collect:
                    result = "看过";
                    break;
                case BangumiFacade.CollectionStatusEnum.@do:
                    result = "在看";
                    break;
                case BangumiFacade.CollectionStatusEnum.on_hold:
                    result = "搁置";
                    break;
                case BangumiFacade.CollectionStatusEnum.dropped:
                    result = "抛弃";
                    break;
                default:
                    result = "收藏";
                    break;
            }
            return result;
        }

        public BangumiFacade.CollectionStatusEnum GetStatusEnum()
        {
            BangumiFacade.CollectionStatusEnum result;
            switch (CollectionStatusText)
            {
                case "想看":
                    result = BangumiFacade.CollectionStatusEnum.wish;
                    break;
                case "看过":
                    result = BangumiFacade.CollectionStatusEnum.collect;
                    break;
                case "在看":
                    result = BangumiFacade.CollectionStatusEnum.@do;
                    break;
                case "搁置":
                    result = BangumiFacade.CollectionStatusEnum.on_hold;
                    break;
                case "抛弃":
                    result = BangumiFacade.CollectionStatusEnum.dropped;
                    break;
                default:
                    result = BangumiFacade.CollectionStatusEnum.@do;
                    break;
            }
            return result;
        }

        public string GetEpisodeType(int n)
        {
            string type = "";
            switch (n)
            {
                case 0:
                    type = "本篇";
                    break;
                case 1:
                    type = "特别篇";
                    break;
                case 2:
                    type = "OP";
                    break;
                case 3:
                    type = "ED";
                    break;
                case 4:
                    type = "预告/宣传/广告";
                    break;
                case 5:
                    type = "MAD";
                    break;
                case 6:
                    type = "其他";
                    break;
                default:
                    break;
            }
            return type;
        }


    }
}
