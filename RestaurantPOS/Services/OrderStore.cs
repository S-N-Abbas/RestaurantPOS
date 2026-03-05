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



        public bool HasOrder(int contextId)
       => _orders.ContainsKey(contextId);
        
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
                _orders[order.ContextId] =
                    new OrderState(order.ContextId, order, RemoveItem);
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
            int contextId,
            Action<OrderItemViewModel> removeCallback)
        {
            if (_orders.TryGetValue(contextId, out var state))
                return state;

            var order = await _orderService.GetOpenOrderAsync(contextId);

            state = new OrderState(contextId, order, removeCallback);
            _orders[contextId] = state;

            return state;
        }

        public void CloseOrder(int contextId)
        {
            if (_orders.Remove(contextId))
                OrderStateChanged?.Invoke(contextId);
        }

        public decimal GetOrderTotal(int contextId)
        {
            _orders.TryGetValue(contextId, out var state);
            if (state == null)
                return 0;

            return state.Order!.ItemsTotal
                + state.Order.ChildCovers * _settingsService.Settings.ChildCoverPrice
                + state.Order.AdultCovers * _settingsService.Settings.AdultCoverPrice;
        }
    }
}
