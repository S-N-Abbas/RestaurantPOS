using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Orders
{
    public class OrderItemViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        public int TableNumber { get; }
        public int ItemId { get; }
        public string Name { get; }
        public decimal UnitPrice { get; }
        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;
        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public decimal Total => Quantity * UnitPrice;

        public ICommand IncreaseCommand { get; }
        public ICommand DecreaseCommand { get; }

        public OrderItemViewModel(MenuItemViewModel item, SettingsService settingsService, Action<OrderItemViewModel> removeCallback)
        {
            ItemId = item.Id;
            Name = item.Name;
            UnitPrice = item.Price;
            _settingsService = settingsService;

            _settingsService.SettingsChanged += () =>
            {
                OnPropertyChanged(nameof(CurrencySymbol));
            };

            IncreaseCommand = new RelayCommand(() => Quantity++);
            DecreaseCommand = new RelayCommand(() =>
            {
                Quantity--;
                if (Quantity == 0)
                    removeCallback(this);
            });
        }

        public OrderItemViewModel(
    OrderItem orderItem, int tableNumber, SettingsService settingsService,
    Action<OrderItemViewModel> removeCallback)
        {
            TableNumber = tableNumber;
            ItemId = orderItem.ProductId;
            Name = orderItem.ProductName;
            _settingsService = settingsService;

            _settingsService.SettingsChanged += () =>
            {
                OnPropertyChanged(nameof(CurrencySymbol));
            };

            UnitPrice = orderItem.UnitPrice;
            _quantity = orderItem.Quantity;

            IncreaseCommand = new RelayCommand(() => Quantity++);
            DecreaseCommand = new RelayCommand(() =>
            {
                Quantity--;
                if (Quantity == 0)
                    removeCallback(this);
            });
        }
    }

}
