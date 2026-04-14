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
        private readonly SettingsService _settingsService;
        public int ContextId { get; }
        public Order? Order { get; private set; }

        public ObservableCollection<OrderItemViewModel> Items { get; }

        public OrderState(int contextId, Order? order, SettingsService settingsService,
            Action<OrderItemViewModel> removeCallback)
        {
            _settingsService = settingsService;
            ContextId = contextId;
            Order = order;

            Items = new ObservableCollection<OrderItemViewModel>(
                order?.Items.Select(i =>
                    new OrderItemViewModel(i, contextId, _settingsService, removeCallback))
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

        /// <summary>
        /// Holds the intended OrderType for a reserved slot that has no DB Order yet.
        /// Once an Order is created and attached, read OrderType from Order directly.
        /// </summary>
        public OrderType? PendingOrderType { get; set; }

        /// <summary>
        /// The effective OrderType — from the live Order if it exists,
        /// otherwise from the reservation. Never null for a valid slot.
        /// </summary>
        public OrderType EffectiveOrderType
            => Order?.OrderType ?? PendingOrderType ?? OrderType.TakeAway;
    }

}
