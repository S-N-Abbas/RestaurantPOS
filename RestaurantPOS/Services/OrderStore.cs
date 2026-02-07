using RestaurantPOS.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class OrderStore
    {
        private readonly OrderService _orderService;
        private readonly Dictionary<int, OrderState> _orders = new();

        public event Action<int>? OrderStateChanged;

        public bool HasOrder(int tableNumber)
       => _orders.ContainsKey(tableNumber);
        
        public OrderStore(OrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<OrderState> GetOrCreateAsync(
            int tableNumber,
            Action<OrderItemViewModel> removeCallback)
        {
            if (_orders.TryGetValue(tableNumber, out var state))
                return state;

            var order = await _orderService.GetOpenOrderAsync(tableNumber);

            state = new OrderState(tableNumber, order, removeCallback);
            _orders[tableNumber] = state;

            return state;
        }

        public void CloseOrder(int tableNumber)
        {
            if (_orders.Remove(tableNumber))
                OrderStateChanged?.Invoke(tableNumber);
        }
    }
}
