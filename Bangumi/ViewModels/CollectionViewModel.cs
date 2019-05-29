using Bangumi.Common;
using Bangumi.Facades;
using Bangumi.Helper;
using Bangumi.Models;
using Bangumi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Bangumi.ViewModels
{
    public class CollectionViewModel : ViewModelBase
    {
        public CollectionViewModel()
        {
            IsLoading = false;
        }

        public ObservableCollection<Collection> SubjectCollection { get; set; } = new ObservableCollection<Collection>();

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

        /// <summary>
        /// 刷新收藏列表，API限制每类最多25条。
        /// </summary>
        public async void LoadCollectionList()
        {
            try
            {
                var subjectType = GetSubjectType();
                if (OAuthHelper.IsLogin)
                {
                    IsLoading = true;
                    HomePage.homePage.isLoading = IsLoading;
                    MainPage.rootPage.RefreshAppBarButton.IsEnabled = false;
                    await BangumiFacade.PopulateSubjectCollectionAsync(SubjectCollection, subjectType);
                }
                else
                {
                    //Message = "请先登录！";
                }
            }
            catch (Exception e)
            {
                var msgDialog = new Windows.UI.Popups.MessageDialog("获取用户收藏失败！\n" + e.Message) { Title = "错误！" };
                msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定"));
                await msgDialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
                HomePage.homePage.isLoading = IsLoading;
                MainPage.rootPage.RefreshAppBarButton.IsEnabled = true;
            }
        }

        private SubjectTypeEnum GetSubjectType()
        {
            switch (SelectedIndex)
            {
                case 0:
                    return SubjectTypeEnum.anime;
                case 1:
                    return SubjectTypeEnum.book;
                case 2:
                    return SubjectTypeEnum.music;
                case 3:
                    return SubjectTypeEnum.game;
                case 4:
                    return SubjectTypeEnum.real;
                default:
                    return SubjectTypeEnum.anime;
            }
        }



    }
}
