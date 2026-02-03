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

        event Action OnCurrentViewModelChanged;
        void NavigateTo<T>() where T : ViewModelBase;
    }
}
