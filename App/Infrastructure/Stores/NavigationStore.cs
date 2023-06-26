using MyApplication.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApplication.Infrastructure.Stores
{
    public class NavigationStore
    {
        public event Action CurrentViewModelChanged;

        private ViewModel _currentViewModel;
        public ViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnCurrenViewModelChanged();
            }
        }

        private void OnCurrenViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
