using RestaurantPOS.Domain.Entities;
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
        public Order? Order { get; private set; }

        public ObservableCollection<OrderItemViewModel> Items { get; }

        public OrderState(int tableNumber, Order? order,
            Action<OrderItemViewModel> removeCallback)
        {
            TableNumber = tableNumber;
            Order = order;

            Items = new ObservableCollection<OrderItemViewModel>(
                order?.Items.Select(i =>
                    new OrderItemViewModel(i, removeCallback))
                ?? Enumerable.Empty<OrderItemViewModel>()
            );
        }

        public void UpdateFrom(Order order)
        {
            Order = order;
        }


        public void AttachOrder(Order order)
        {
            Order = order;
        }
    }

}
