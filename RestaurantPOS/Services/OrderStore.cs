using RestaurantPOS.Domain.Entities;
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
            if (state == null || state.Order == null)
                return 0;

            return state.Order!.ItemsTotal
                + state.Order.ChildCovers * _settingsService.Settings.ChildCoverPrice
                + state.Order.AdultCovers * _settingsService.Settings.AdultCoverPrice;
        }

        /// <summary>
        /// Returns the OrderType of the active order in a given context slot.
        /// Returns DineIn as a safe default if no order exists.
        /// </summary>
        public OrderType GetOrderType(int contextId)
        {
            if (_orders.TryGetValue(contextId, out var state))
                return state.EffectiveOrderType;

            return OrderType.DineIn;
        }

        /// <summary>
        /// Returns all currently active negative ContextIds that match the given OrderType.
        /// Used by OrderSwitcherViewModel to build the slot lists.
        /// </summary>
        public IReadOnlyList<int> GetActiveSlots(OrderType orderType)
        {
            return _orders
                .Where(kvp =>
                    kvp.Key < 0 &&
                    kvp.Value.EffectiveOrderType == orderType) // ✅ was: kvp.Value.Order?.OrderType == orderType
                .Select(kvp => kvp.Key)
                .OrderBy(id => id)
                .ToList();
        }

        /// <summary>
        /// Reserves a slot in the store with no Order record yet.
        /// This makes the slot visible in the switcher immediately on creation,
        /// before the user adds the first item (which triggers actual DB order creation).
        /// </summary>
        public void ReserveSlot(int contextId, OrderType orderType)
        {
            if (_orders.ContainsKey(contextId))
                return;

            // Create an OrderState with a null Order — same pattern as dine-in tables
            // that have no active order. The VM handles null Order gracefully already.
            var state = new OrderState(contextId, null, RemoveItem);

            // Stamp the intended OrderType so the slot label renders correctly
            // before the DB record exists
            state.PendingOrderType = orderType;   // see OrderState addition below

            _orders[contextId] = state;
            OrderStateChanged?.Invoke(contextId);
        }

        /// <summary>
        /// Allows OrderViewModel to signal the switcher after a lazy-created
        /// order is attached, so slot lists refresh immediately.
        /// </summary>
        public void NotifyOrderStateChanged(int contextId)
            => OrderStateChanged?.Invoke(contextId);
    }
}
