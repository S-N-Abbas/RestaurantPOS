using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class OrderContextService : IOrderContextService
    {
        private OrderStore _orderStore;
        public int CurrentContext { get; private set; }
        public OrderType CurrentOrderType { get; private set; } = OrderType.DineIn;

        public event Action<int>? ContextChanged;

        public OrderContextService(OrderStore orderStore)
        {
            CurrentContext = 1; // default to table 1
            _orderStore = orderStore;
        }

        /// <summary>
        /// Switch to another order.
        /// </summary>
        public void SwitchContext(int contextId)
        {
            if (contextId == 0)
                throw new ArgumentException("ContextId cannot be zero.", nameof(contextId));

            if (CurrentContext == contextId)
                return;

            CurrentContext = contextId;
            CurrentOrderType = contextId > 0
                ? OrderType.DineIn
                : _orderStore.GetOrderType(contextId); // reads TakeAway or Delivery from the reservation

            ContextChanged?.Invoke(CurrentContext);
        }

    }
}
