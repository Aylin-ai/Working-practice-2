using App.Infrastructure.Stores;
using MyApplication.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApplication.ViewModels
{
    public class FamilyViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        public FamilyViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            #region Команды



            #endregion
        }
    }
}
