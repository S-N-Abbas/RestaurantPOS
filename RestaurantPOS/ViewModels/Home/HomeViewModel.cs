using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Login;
using RestaurantPOS.ViewModels.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantPOS.ViewModels.BackOffice.Users;

namespace RestaurantPOS.ViewModels.Home
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly AuthorizationService _authorizationService;
        public IRelayCommand DineInCommand { get; }
        public IRelayCommand TakeawayCommand { get; }
        public IRelayCommand BackOfficeCommand { get; }

        public HomeViewModel(INavigationService navigationService, AuthorizationService authorizationService)
        {
            _navigationService = navigationService;
            _authorizationService = authorizationService;

            DineInCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<TablesViewModel>());

            TakeawayCommand = new RelayCommand(() => Navigate("Takeaway"));
            BackOfficeCommand = new RelayCommand(GoToBackOffice, CanGoToBackOffice);
        }

        private bool CanGoToBackOffice()
        {
            return _authorizationService.HasAccess(Domain.Entities.UserRole.Admin);
        }

        private void GoToBackOffice()
        {
            _navigationService.NavigateTo<UsersViewModel>();
        }

        private void Navigate(string mode)
        {
            // Later:
            // - Tables for DineIn
            // - Order screen for Takeaway / Delivery
        }

    }
}
