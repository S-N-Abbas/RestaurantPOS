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
        private readonly SettingsService _settingsService;
        private readonly Dictionary<int, OrderState> _orders = new();

        public event Action<int>? OrderStateChanged;



        public bool HasOrder(int tableNumber)
       => _orders.ContainsKey(tableNumber);
        
        public OrderStore(OrderService orderService, SettingsService settingsService)
        {
            _orderService = orderService;
            _settingsService = settingsService;
        }

        public async Task InitializeAsync()
        {
            var openOrders = await _orderService.GetOpenOrdersAsync();

            foreach (var order in openOrders)
            {
                _orders[order.TableNumber] =
                    new OrderState(order.TableNumber, order, RemoveItem);
            }
        }

        private void RemoveItem(OrderItemViewModel item)
        {
            if (item == null) return;

            if (_orders.TryGetValue(item.TableNumber, out var orderState))
            {
                orderState.Items.Remove(item);
            }
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

        public decimal GetOrderTotal(int number)
        {
            _orders.TryGetValue(number, out var state);
            if (state == null)
                return 0;

            return state.Order!.ItemsTotal
                + state.Order.ChildCovers * _settingsService.Settings.ChildCoverPrice
                + state.Order.AdultCovers * _settingsService.Settings.AdultCoverPrice;
        }
    }
}
