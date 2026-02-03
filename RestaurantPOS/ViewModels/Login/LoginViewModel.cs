using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Login
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public IRelayCommand LoginCommand { get; }

        public LoginViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            LoginCommand = new RelayCommand(OnLogin);
        }

        private void OnLogin()
        {
            // Later: validate PIN / user
            _navigationService.NavigateTo<HomeViewModel>();
        }
    }
}
