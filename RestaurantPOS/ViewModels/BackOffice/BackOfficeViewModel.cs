using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.BackOffice.Users;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantPOS.ViewModels.BackOffice.Settings;
using RestaurantPOS.ViewModels.Orders.History;

namespace RestaurantPOS.ViewModels.BackOffice
{
    public class BackOfficeViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly AuthorizationService _authorizationService;
        public IRelayCommand UsersCommand { get; }
        public IRelayCommand SettingsCommand { get; }
        public IRelayCommand OrderHistoryCommand { get; }

        public BackOfficeViewModel(INavigationService navigationService, AuthorizationService authorizationService)
        {
            _navigationService = navigationService;
            _authorizationService = authorizationService;
            UsersCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<UsersViewModel>());
            
            OrderHistoryCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<OrderHistoryViewModel>());

            SettingsCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<SettingsViewModel>());
        }
    }
}
