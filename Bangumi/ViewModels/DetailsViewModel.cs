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

        private string _imageSource = "ms-appx:///Assets/NoImage.png";
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


        // 更多资料用
        public string name;
        public string moreInfo;
        public string moreSummary;
        public string moreCharacters;
        public string moreStaff;

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
            // 更多资料用
            name = "";
            moreInfo = "";
            moreSummary = "";
            moreCharacters = "";
            moreStaff = "";
            // 收藏状态，评分，吐槽
            CollectionStatusText = "收藏";
            CollectionStatusIcon = "\uE006";
            myRate = 0;
            myComment = "";
            myPrivacy = false;

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
                    if (collectionEditContentDialog.collectionStatus == CollectionStatusEnum.collect.GetValue() && SettingHelper.EpsBatch == true)
                    {
                        int epId = 0;
                        string epsId = string.Empty;
                        foreach (var episode in eps)
                        {
                            if (eps.IndexOf(episode) == eps.Count - 1)
                            {
                                epsId += episode.Id.ToString();
                                epId = episode.Id;
                                break;
                            }
                            else
                            {
                                epsId += episode.Id.ToString() + ",";
                            }
                        }
                        if (await BangumiFacade.UpdateProgressBatchAsync(epId, EpStatusEnum.watched, epsId))
                        {
                            foreach (var episode in eps)
                            {
                                episode.Status = "看过";
                            }
                        }
                    }
                }
                IsStatusLoaded = true;
                IsUpdating = false;
            }
        }

        /// <summary>
        /// 显示更多资料
        /// </summary>
        public async void ShowMoreInfo()
        {
            SubjectMoreInfoContentDialog subjectMoreInfoContentDialog = new SubjectMoreInfoContentDialog()
            {
                name = name,
                info = moreInfo,
                summary = moreSummary,
                characters = moreCharacters,
                staff = moreStaff,
            };
            subjectMoreInfoContentDialog.Title = NameCn;
            MainPage.rootPage.hasDialog = true;
            await subjectMoreInfoContentDialog.ShowAsync();
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
                    if (episode.Id == ep.Id)
                    {
                        epsId += episode.Id.ToString();
                        break;
                    }
                    else
                    {
                        epsId += episode.Id.ToString() + ",";
                    }
                }
                if (await BangumiFacade.UpdateProgressBatchAsync(ep.Id, status, epsId))
                {
                    foreach (var episode in eps)
                    {
                        episode.Status = "看过";
                        if (episode.Id == ep.Id)
                        {
                            break;
                        }
                    }
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

                if (subject != null)
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

                    // 更多资料
                    name = subject.Name;
                    moreSummary = string.IsNullOrEmpty(subject.Summary) ? "暂无简介" : subject.Summary;
                    moreInfo = "作品分类：" + ((SubjectTypeEnum)subject.Type).GetValue();
                    moreInfo += "\n放送开始：" + subject.AirDate;
                    moreInfo += "\n放送星期：" + Converters.GetWeekday(subject.AirWeekday);
                    moreInfo += "\n话数：" + subject.EpsCount;
                    // 角色
                    if (subject.Characters != null)
                    {
                        moreCharacters = "";
                        foreach (var crt in subject.Characters)
                        {
                            moreCharacters += string.Format("{0}：", string.IsNullOrEmpty(crt.NameCn) ? crt.Name : crt.NameCn);
                            if (crt.Actors != null)
                            {
                                foreach (var actor in crt.Actors)
                                {
                                    moreCharacters += actor.Name + "、";
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
                        moreCharacters = "暂无资料";
                    }
                    // 演职人员
                    if (subject.Staff != null)
                    {
                        moreStaff = "";
                        var sd = new Dictionary<string, string>();
                        foreach (var staff in subject.Staff)
                        {
                            foreach (var job in staff.Jobs)
                            {
                                if (!sd.ContainsKey(job))
                                {
                                    sd.Add(job, string.IsNullOrEmpty(staff.NameCn) ? staff.Name : staff.NameCn);
                                }
                                else
                                {
                                    sd[job] += string.Format("、{0}", string.IsNullOrEmpty(staff.NameCn) ? staff.Name : staff.NameCn);
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
                        moreStaff = "暂无资料";
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
}
