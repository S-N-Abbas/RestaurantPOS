using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class OrderStore
    {
        private readonly Dictionary<int, OrderState> _orders = new();

        public event Action<int>? OrderStateChanged;
        public OrderState GetOrCreate(int tableNumber)
        {
            if (!_orders.TryGetValue(tableNumber, out var order))
            {
                order = new OrderState(tableNumber);
                _orders[tableNumber] = order;
                OrderStateChanged?.Invoke(tableNumber);
            }

            return order;
        }

        public void CloseOrder(int tableNumber)
        {
            if (_orders.Remove(tableNumber))
                OrderStateChanged?.Invoke(tableNumber);
        }
    }
}
