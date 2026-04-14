using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Tables;
using RestaurantPOS.ViewModels.Takeaway;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Orders
{
    /// <summary>
    /// Unified order switcher overlay. Replaces TablePickerViewModel.
    /// Shows three sections: Dine-In tables, active TakeAway slots,
    /// and active Delivery slots. Also provides commands to open new
    /// TakeAway and Delivery orders.
    /// </summary>
    public class OrderSwitcherViewModel : ViewModelBase
    {
        private readonly IOrderContextService _orderContextService;
        private readonly OrderStore _orderStore;
        private readonly TableStore _tableStore;
        private readonly SettingsService _settingsService;

        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        // ─── Collections ─────────────────────────────────────────────────────────

        /// <summary>All active dine-in tables (from TableStore — persisted config).</summary>
        public ObservableCollection<TableViewModel> Tables
            => _tableStore.Tables;

        /// <summary>Only currently open TakeAway slots (built dynamically from OrderStore).</summary>
        public ObservableCollection<TakeAwaySlotViewModel> TakeAwaySlots { get; } = new();

        /// <summary>Only currently open Delivery slots (built dynamically from OrderStore).</summary>
        public ObservableCollection<TakeAwaySlotViewModel> DeliverySlots { get; } = new();

        // ─── Visibility Helpers ───────────────────────────────────────────────────

        /// <summary>Collapse the TakeAway section header when no slots are open.</summary>
        public bool HasTakeAwaySlots => TakeAwaySlots.Any();

        /// <summary>Collapse the Delivery section header when no slots are open.</summary>
        public bool HasDeliverySlots => DeliverySlots.Any();

        // ─── Commands ────────────────────────────────────────────────────────────

        /// <summary>Opens a brand-new TakeAway order in the next free slot.</summary>
        public ICommand NewTakeAwayCommand { get; }

        /// <summary>Opens a brand-new Delivery order in the next free slot.</summary>
        public ICommand NewDeliveryCommand { get; }

        // ─── Constructor ─────────────────────────────────────────────────────────

        public OrderSwitcherViewModel(
            IOrderContextService orderContextService,
            OrderStore orderStore,
            TableStore tableStore,
            SettingsService settingsService)
        {
            _orderContextService = orderContextService;
            _orderStore = orderStore;
            _tableStore = tableStore;
            _settingsService = settingsService;
            NewTakeAwayCommand = new RelayCommand(
                () => OpenNewSlot(OrderType.TakeAway));

            NewDeliveryCommand = new RelayCommand(
                () => OpenNewSlot(OrderType.Delivery));

            // ✅ When any order closes or opens, rebuild the slot lists
            _orderStore.OrderStateChanged += _ => RefreshSlots();

            _settingsService.SettingsChanged += () => OnPropertyChanged(nameof(CurrencySymbol));

            _ = InitialiseAsync();
        }

        // ─── Initialisation ──────────────────────────────────────────────────────

        private async Task InitialiseAsync()
        {
            await _tableStore.LoadAsync();
            RefreshSlots();
        }

        // ─── Slot Management ─────────────────────────────────────────────────────

        /// <summary>
        /// Rebuilds TakeAway and Delivery slot collections from the live OrderStore.
        /// Only slots that currently have an open order are shown.
        /// Called on startup and whenever an order opens or closes.
        /// </summary>
        private void RefreshSlots()
        {
            RebuildSlots(TakeAwaySlots, OrderType.TakeAway);
            RebuildSlots(DeliverySlots, OrderType.Delivery);

            OnPropertyChanged(nameof(HasTakeAwaySlots));
            OnPropertyChanged(nameof(HasDeliverySlots));
        }

        private void RebuildSlots(
            ObservableCollection<TakeAwaySlotViewModel> collection,
            OrderType targetType)
        {
            // Get all active negative contextIds matching the target OrderType
            var activeContextIds = _orderStore
                .GetActiveSlots(targetType);   // new helper — see OrderStore addition below

            // ── Remove slots that are no longer open ──
            var toRemove = collection
                .Where(s => !activeContextIds.Contains(s.ContextId))
                .ToList();

            foreach (var slot in toRemove)
                collection.Remove(slot);

            // ── Add newly opened slots ──
            foreach (var contextId in activeContextIds)
            {
                if (collection.All(s => s.ContextId != contextId))
                {
                    collection.Add(new TakeAwaySlotViewModel(
                        contextId,
                        _orderContextService,
                        _orderStore,
                        _settingsService));
                }
            }
        }

        /// <summary>
        /// Finds the next free negative ContextId slot and switches context into it.
        /// The OrderViewModel will create the actual Order record when the first
        /// item is added (same lazy-creation pattern as dine-in).
        /// </summary>
        private void OpenNewSlot(OrderType orderType)
        {
            int slot = GetNextFreeSlotNumber();
            int contextId = -slot;

            // Register an empty placeholder in the store so the slot
            // appears immediately in the switcher before any item is added
            _orderStore.ReserveSlot(contextId, orderType); // new helper — see below

            _orderContextService.SwitchContext(contextId);

            RefreshSlots();
        }

        /// <summary>
        /// Returns the next positive slot number whose negative equivalent
        /// is not already in use in the OrderStore.
        /// </summary>
        private int GetNextFreeSlotNumber()
        {
            int slot = 1;
            while (_orderStore.HasOrder(-slot))
                slot++;
            return slot;
        }
    }
}
