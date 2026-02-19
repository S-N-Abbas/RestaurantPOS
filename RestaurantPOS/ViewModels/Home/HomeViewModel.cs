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
        public IRelayCommand OrderCommand { get; }
        public IRelayCommand ReportsCommand { get; }
        public IRelayCommand BackOfficeCommand { get; }

        public HomeViewModel(INavigationService navigationService, AuthorizationService authorizationService)
        {
            _navigationService = navigationService;
            _authorizationService = authorizationService;

            OrderCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<TablesViewModel>());

            ReportsCommand = new RelayCommand(GoToReports, CanGoToReports);

            BackOfficeCommand = new RelayCommand(GoToBackOffice, CanGoToBackOffice);
        }

        private bool CanGoToReports()
        {
            return _authorizationService.HasAccess(Domain.Entities.UserRole.Manager, Domain.Entities.UserRole.Admin);
        }

        private void GoToReports()
        {
            throw new NotImplementedException();
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
