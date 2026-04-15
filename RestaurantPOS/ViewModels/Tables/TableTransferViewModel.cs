using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Tables
{
    public class TableTransferViewModel : ViewModelBase
    {
        private readonly OrderStore _orderStore;
        private readonly TableStore _tableStore;
        private readonly int _currentContextId;

        // ─── Collections ─────────────────────────────────────────────────────

        /// <summary>All tables except the current one.</summary>
        public ObservableCollection<TableTransferSlotViewModel> Tables { get; } = new();

        // ─── Events ───────────────────────────────────────────────────────────

        /// <summary>Fired when user confirms a transfer. Payload is destination table number.</summary>
        public event Action<int>? TransferRequested;

        public event Action? Cancelled;

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand SelectTableCommand { get; }
        public ICommand CancelCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public TableTransferViewModel(
            int currentContextId,
            OrderStore orderStore,
            TableStore tableStore)
        {
            _currentContextId = currentContextId;
            _orderStore = orderStore;
            _tableStore = tableStore;

            SelectTableCommand = new RelayCommand<TableTransferSlotViewModel>(OnSelectTable);
            CancelCommand = new RelayCommand(() => Cancelled?.Invoke());

            BuildTableList();

            // Refresh if any order opens or closes while picker is open
            _orderStore.OrderStateChanged += _ => BuildTableList();
        }

        // ─── Private ──────────────────────────────────────────────────────────

        private void BuildTableList()
        {
            Tables.Clear();

            foreach (var table in _tableStore.Tables)
            {
                // Exclude the current table itself
                if (table.tableNumber == _currentContextId)
                    continue;

                Tables.Add(new TableTransferSlotViewModel(
                    table.tableNumber,
                    _orderStore.HasOrder(table.tableNumber)));
            }
        }

        private void OnSelectTable(TableTransferSlotViewModel? slot)
        {
            if (slot == null || slot.IsOccupied) return;
            TransferRequested?.Invoke(slot.TableNumber);
        }
    }
}
