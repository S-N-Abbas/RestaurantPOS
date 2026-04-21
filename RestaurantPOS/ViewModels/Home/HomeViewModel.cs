using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.BackOffice;
using RestaurantPOS.ViewModels.BackOffice.Users;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Login;
using RestaurantPOS.ViewModels.Orders;
using RestaurantPOS.ViewModels.Tables;
using RestaurantPOS.ViewModels.ZReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Home
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly AuthorizationService _authorizationService;
        private readonly IOrderContextService _orderContextService;
        private readonly OrderStore _orderStore;

        public IRelayCommand OrderCommand { get; }

        public IRelayCommand TakeawayCommand { get; }
        public IRelayCommand ReportsCommand { get; }
        public IRelayCommand BackOfficeCommand { get; }


        public HomeViewModel(INavigationService navigationService,
            AuthorizationService authorizationService,
            IOrderContextService orderContextService,
            OrderStore orderStore)
        {
            _navigationService = navigationService;
            _authorizationService = authorizationService;
            _orderContextService = orderContextService;
            _orderStore = orderStore;

            OrderCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<TablesViewModel>());

            TakeawayCommand = new RelayCommand(GoToTakeaway);

            ReportsCommand = new RelayCommand(GoToZReport, CanGoToReports);

            BackOfficeCommand = new RelayCommand(GoToBackOffice, CanGoToBackOffice);
        }
        private void GoToTakeaway()
        {
            // Find next free negative slot — same logic as OrderSwitcherViewModel
            int slot = 1;
            while (_orderStore.HasOrder(-slot))
                slot++;

            int contextId = -slot;

            // Reserve the slot as TakeAway so OrderContextService
            // can read its OrderType before a DB order exists
            _orderStore.ReserveSlot(contextId, OrderType.TakeAway);

            _orderContextService.SwitchContext(contextId);

            _navigationService.NavigateTo<OrderViewModel>();
        }
        private bool CanGoToReports()
        {
            return _authorizationService.HasAccess(Domain.Entities.UserRole.Manager, Domain.Entities.UserRole.Admin);
        }

        private void GoToZReport()
        {
            _navigationService.NavigateTo<ZReportViewModel>();
        }

        private bool CanGoToBackOffice()
        {
            return _authorizationService.HasAccess(Domain.Entities.UserRole.Admin);
        }

        private void GoToBackOffice()
        {
            _navigationService.NavigateTo<BackOfficeViewModel>();
        }
    }
}
