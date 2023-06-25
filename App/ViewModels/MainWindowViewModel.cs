using MyApplication.ViewModels.Base;
using MyApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Infrastructure.Stores;

namespace MyApplication.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;
        private readonly NavigationStore _navigationStore;

        public ViewModel MainMenuViewModel { get; set; }
        public ViewModel CurrentViewModel => _navigationStore.CurrentViewModel;

        public MainWindowViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
