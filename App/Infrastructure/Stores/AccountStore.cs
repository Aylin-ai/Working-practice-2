using MyApplication.Models;
using MyApplication.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApplication.Infrastructure.Stores
{
    public class AccountStore : ViewModel
    {
        private Account _currentAccount;
        public Account CurrentAccount { get => _currentAccount; set => Set(ref _currentAccount, value); }
    }
}
