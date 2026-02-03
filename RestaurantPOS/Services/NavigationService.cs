using Microsoft.Extensions.DependencyInjection;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged?.Invoke();
            }
        }

        public event Action OnCurrentViewModelChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<T>() where T : ViewModelBase
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<T>();
        }
    }
}
