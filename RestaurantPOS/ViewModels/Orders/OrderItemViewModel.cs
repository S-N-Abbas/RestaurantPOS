using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
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
        public int TableNumber { get; }
        public int ItemId { get; }
        public string Name { get; }
        public decimal UnitPrice { get; }

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

        public OrderItemViewModel(MenuItemViewModel item, Action<OrderItemViewModel> removeCallback)
        {
            ItemId = item.Id;
            Name = item.Name;
            UnitPrice = item.Price;

            IncreaseCommand = new RelayCommand(() => Quantity++);
            DecreaseCommand = new RelayCommand(() =>
            {
                Quantity--;
                if (Quantity == 0)
                    removeCallback(this);
            });
        }

        public OrderItemViewModel(
    OrderItem orderItem, int tableNumber,
    Action<OrderItemViewModel> removeCallback)
        {
            TableNumber = tableNumber;
            ItemId = orderItem.ProductId;
            Name = orderItem.ProductName;
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
