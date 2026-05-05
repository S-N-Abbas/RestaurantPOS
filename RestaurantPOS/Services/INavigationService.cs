using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface INavigationService
    {
        ViewModelBase CurrentViewModel { get; }

        event Action? CurrentViewModelChanged;

        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
        void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;

        void GoBack();
        bool CanGoBack { get; }

        void ClearHistory();
    }
}
