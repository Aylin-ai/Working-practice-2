﻿using MyApplication.Infrastructure.Stores;
using MyApplication.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApplication.ViewModels
{
    public class ApplicationViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        public ApplicationViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            #region Команды



            #endregion
        }
    }
}
