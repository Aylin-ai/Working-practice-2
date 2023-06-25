using App.Infrastructure.Commands;
using App.Infrastructure.Stores;
using App.Services;
using Google.Protobuf.Collections;
using MyApplication.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApplication.ViewModels
{
    public class ProfileViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        public ProfileViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            #region Команды



            #endregion
        }
    }
}
