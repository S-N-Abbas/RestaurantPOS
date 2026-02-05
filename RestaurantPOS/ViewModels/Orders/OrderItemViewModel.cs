using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Orders
{
    public class OrderItemViewModel : ViewModelBase
    {
        public int ItemId { get; }
        public string Name { get; }
        public decimal UnitPrice { get; }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                    OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Total => Quantity * UnitPrice;

        public OrderItemViewModel(MenuItemViewModel item)
        {
            ItemId = item.Id;
            Name = item.Name;
            UnitPrice = item.Price;
        }
    }

}
