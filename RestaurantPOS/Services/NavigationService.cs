using CommunityToolkit.Mvvm.ComponentModel;
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
        private readonly Stack<ViewModelBase> _history = new();

        private readonly IServiceProvider _serviceProvider;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                CurrentViewModelChanged?.Invoke();
            }
        }

        public bool CanGoBack => _history.Count > 1;

        public event Action? CurrentViewModelChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            if (CurrentViewModel != null)
                _history.Push(CurrentViewModel);
            CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
        }

        public void NavigateTo<TViewModel>(object parameter)
            where TViewModel : ViewModelBase
        {
            if (CurrentViewModel != null)
                _history.Push(CurrentViewModel);
            CurrentViewModel = (ViewModelBase)ActivatorUtilities
                .CreateInstance(_serviceProvider, typeof(TViewModel), parameter);
        }

        public void GoBack()
        {
            if (_history.Count > 0)
            {
                var previous = _history.Pop();
                // Logic to set CurrentViewModel to previous without pushing to history
                CurrentViewModel = previous;
            }
        }

        public void NavigateBackTo<TViewModel>() where TViewModel : ViewModelBase
        {
            // Search history for an existing instance of the target ViewModel
            while (_history.Count > 0)
            {
                var top = _history.Pop();
                if (top is TViewModel)
                {
                    CurrentViewModel = top;
                    return;
                }
            }

            // If not found in history, just navigate to a fresh one
            NavigateTo<TViewModel>();
        }

        public void ResetTo<TViewModel>() where TViewModel : ViewModelBase
        {
            // Clear everything and start fresh at a specific view
            _history.Clear();
            CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
        }

        public void ClearHistory()
        {
            _history.Clear();
        }
    }
}
