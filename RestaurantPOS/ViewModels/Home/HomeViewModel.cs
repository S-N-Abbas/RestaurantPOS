using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
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

        public IRelayCommand DineInCommand { get; }
        public IRelayCommand TakeawayCommand { get; }
        public IRelayCommand DeliveryCommand { get; }

        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            DineInCommand = new RelayCommand(() => Navigate("DineIn"));
            TakeawayCommand = new RelayCommand(() => Navigate("Takeaway"));
            DeliveryCommand = new RelayCommand(() => Navigate("Delivery"));
        }

        private void Navigate(string mode)
        {
            // Later:
            // - Tables for DineIn
            // - Order screen for Takeaway / Delivery
        }

    }
}
