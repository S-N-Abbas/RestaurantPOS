using RestaurantPOS.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class OrderState
    {
        public int TableNumber { get; }
        public ObservableCollection<OrderItemViewModel> Items { get; }

        public OrderState(int tableNumber)
        {
            TableNumber = tableNumber;
            Items = new ObservableCollection<OrderItemViewModel>();
        }
    }

}
