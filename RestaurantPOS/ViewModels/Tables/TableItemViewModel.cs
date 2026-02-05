using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Tables
{
    public class TableItemViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public int TableNumber { get; }
        public bool IsOccupied { get; }

        public IRelayCommand SelectTableCommand { get; }

        public TableItemViewModel(
            int tableNumber,
            bool isOccupied,
            INavigationService navigationService)
        {
            TableNumber = tableNumber;
            IsOccupied = isOccupied;
            _navigationService = navigationService;

            SelectTableCommand = new RelayCommand(OnSelectTable);
        }

        private void OnSelectTable()
        {
            _navigationService.NavigateTo<OrderViewModel>(TableNumber);
        }
    }
}
