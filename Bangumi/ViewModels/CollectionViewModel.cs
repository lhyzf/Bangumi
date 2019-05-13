using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.ViewModels
{
   public class CollectionViewModel : ViewModelBase
    {
        public CollectionViewModel()
        {
            IsLoading = false;
        }

        public ObservableCollection<Collect> subjectCollection { get; private set; } = new ObservableCollection<Collect>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        /// <summary>
        /// 刷新收藏列表，API限制每类最多25条。
        /// </summary>
        public async void LoadCollectionList()
        {
            var subjectType = GetSubjectType();
            if (OAuthHelper.IsLogin)
            {
                IsLoading = true;
                if (await BangumiFacade.PopulateSubjectCollectionAsync(subjectCollection, subjectType))
                {
                    Message = "更新时间：" + DateTime.Now;
                }
                else
                {
                    Message = "获取用户进度失败，请重试或重新登录！";
                }
            }
            else
            {
                Message = "请先登录！";
            }
            IsLoading = false;
        }

        private BangumiFacade.SubjectType GetSubjectType()
        {
            BangumiFacade.SubjectType subjectType = BangumiFacade.SubjectType.anime;
            switch (SelectedIndex)
            {
                case 0:
                    subjectType = BangumiFacade.SubjectType.anime;
                    break;
                case 1:
                    subjectType = BangumiFacade.SubjectType.book;
                    break;
                case 2:
                    subjectType = BangumiFacade.SubjectType.music;
                    break;
                case 3:
                    subjectType = BangumiFacade.SubjectType.game;
                    break;
                case 4:
                    subjectType = BangumiFacade.SubjectType.real;
                    break;
                default:
                    break;
            }
            return subjectType;
        }



    }
}
