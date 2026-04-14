using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Takeaway
{
    /// <summary>
    /// Represents a single active TakeAway or Delivery slot in the Order Switcher.
    /// ContextId is always negative (e.g. -1, -2) to avoid collision with table numbers.
    /// OrderType (TakeAway / Delivery) is read from the live Order record.
    /// </summary>
    public class TakeAwaySlotViewModel : ViewModelBase
    {
        private readonly OrderStore _orderStore;
        private readonly IOrderContextService _orderContextService;
        private readonly SettingsService _settingsService;

        // ─── Identity ────────────────────────────────────────────────────────────

        /// <summary>Negative ContextId, e.g. -1, -2, -3.</summary>
        public int ContextId { get; }

        /// <summary>Human-readable slot number (always positive). e.g. ContextId -2 → SlotNumber 2.</summary>
        public int SlotNumber => Math.Abs(ContextId);

        // ─── Derived State ───────────────────────────────────────────────────────

        public OrderType SlotOrderType
            => _orderStore.GetOrderType(ContextId);  // see note below

        /// <summary>"TakeAway" or "Delivery" — used for section label and icon binding.</summary>
        public string TypeLabel => SlotOrderType switch
        {
            OrderType.TakeAway => "TakeAway",
            OrderType.Delivery => "Delivery",
            _ => "Order"
        };

        /// <summary>Display label shown on the slot card, e.g. "TA #1" or "DEL #2".</summary>
        public string SlotLabel => SlotOrderType switch
        {
            OrderType.TakeAway => $"TA #{SlotNumber}",
            OrderType.Delivery => $"DEL #{SlotNumber}",
            _ => $"#{SlotNumber}"
        };

        public bool IsCurrent
            => _orderContextService.CurrentContext == ContextId;

        /// <summary>
        /// TakeAway/Delivery slots are never "locked" — there is no cross-terminal
        /// ownership concept for them. Any operator can switch into any open slot.
        /// </summary>
        public bool IsLocked => false;

        public decimal CurrentTotal
            => _orderStore.GetOrderTotal(ContextId);

        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        // ─── Commands ────────────────────────────────────────────────────────────

        public ICommand SelectSlotCommand { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────

        public TakeAwaySlotViewModel(
            int contextId,
            IOrderContextService orderContextService,
            OrderStore orderStore,
            SettingsService settingsService)
        {
            if (contextId >= 0)
                throw new ArgumentException(
                    "TakeAwaySlotViewModel requires a negative ContextId.", nameof(contextId));

            ContextId = contextId;
            _orderContextService = orderContextService;
            _orderStore = orderStore;
            _settingsService = settingsService;

            settingsService.SettingsChanged += () => OnPropertyChanged(nameof(CurrencySymbol));

            SelectSlotCommand = new RelayCommand(SelectSlot);

            // ✅ React to order lifecycle changes (order closed, total updated)
            _orderStore.OrderStateChanged += OnOrderStateChanged;

            // ✅ React to context switches (highlight current slot)
            _orderContextService.ContextChanged += _ => RaiseAll();
        }

        // ─── Private Handlers ────────────────────────────────────────────────────

        private void SelectSlot()
            => _orderContextService.SwitchContext(ContextId);

        private void OnOrderStateChanged(int contextId)
        {
            if (contextId == ContextId)
                RaiseAll();
        }

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(IsCurrent));
            OnPropertyChanged(nameof(CurrentTotal));
            OnPropertyChanged(nameof(SlotLabel));
            OnPropertyChanged(nameof(TypeLabel));
            OnPropertyChanged(nameof(SlotOrderType));
            ((RelayCommand)SelectSlotCommand).NotifyCanExecuteChanged();
        }
    }
}
