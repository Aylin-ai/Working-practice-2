using MyApplication.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApplication.Models
{
    public class Account : ViewModel
    {
        private string _telephoneNumber;
        public string TelephoneNumber { get => _telephoneNumber; set => Set(ref _telephoneNumber, value); }

        private string _password;
        public string Password { get => _password; set => Set(ref _password, value); }
    }
}
