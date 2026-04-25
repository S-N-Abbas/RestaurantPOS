using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Cover;
using RestaurantPOS.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
namespace RestaurantPOS.ViewModels.Tables
{
    public class TablesViewModel : ViewModelBase
    {
        public ObservableCollection<TableViewModel> Tables => _tableStore.Tables;

        private readonly TableStore _tableStore;
        private readonly ITableService _tableService;
        private readonly IOrderContextService _tableSession;
        private readonly INavigationService _navigation;
        private readonly AuthorizationService _authorizationService;
        private readonly OrderStore _orderStore;

        public TableEditorViewModel Editor { get; }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                    OnPropertyChanged(nameof(EditModeButtonLabel));
            }
        }

        public string EditModeButtonLabel
            => IsEditMode ? "Done" : "Edit Tables";

        // ─── Permissions ──────────────────────────────────────────────────────

        public bool CanManageTables =>
            _authorizationService.HasAccess(UserRole.Admin, UserRole.Manager);

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand SelectTableCommand { get; }
        public ICommand AddTableCommand { get; }
        public ICommand EditTableCommand { get; }
        public ICommand DeleteTableCommand { get; }
        public ICommand ToggleEditModeCommand { get; }

        public TablesViewModel(
         TableStore tableStore,
            ITableService tableService,
            IOrderContextService tableSession,
            INavigationService navigation,
            AuthorizationService authorizationService,
            OrderStore orderStore)
        {
            _tableStore = tableStore;
            _tableService = tableService;
            _tableSession = tableSession;
            _navigation = navigation;
            _authorizationService = authorizationService;
            _orderStore = orderStore;

            Editor = new TableEditorViewModel(tableService);
            Editor.SavedSuccessfully += async _ => await ReloadTablesAsync();

            SelectTableCommand = new RelayCommand<TableViewModel>(OnSelectTable);

            AddTableCommand = new RelayCommand(
                () => Editor.OpenForNew(),
                () => CanManageTables);

            EditTableCommand = new RelayCommand<TableViewModel>(
                OnEditTable,
                _ => CanManageTables);

            DeleteTableCommand = new RelayCommand<TableViewModel>(
                async t => await OnDeleteTableAsync(t),
                _ => CanManageTables);

            ToggleEditModeCommand = new RelayCommand(
                () => IsEditMode = !IsEditMode,
                () => CanManageTables);

            _ = LoadTablesAsync();
        }

        // ─── Load ─────────────────────────────────────────────────────────────

        private async Task LoadTablesAsync()
        {
            await _tableStore.LoadAsync();
        }

        private async Task ReloadTablesAsync()
        {
            await _tableStore.LoadAsync();
        }

        // ─── Actions ──────────────────────────────────────────────────────────

        private void OnSelectTable(TableViewModel? table)
        {
            if (table == null || IsEditMode) return;

            _tableSession.SwitchContext(table.tableNumber);
            _navigation.NavigateTo<OrderViewModel>();
        }

        private void OnEditTable(TableViewModel? table)
        {
            if (table == null) return;

            // We need the DB Id — look it up by table number
            _ = OpenEditForTableAsync(table.tableNumber);
        }

        private async Task OpenEditForTableAsync(int tableNumber)
        {
            var tables = await _tableService.GetAllAsync();
            var match = tables.FirstOrDefault(t => t.Number == tableNumber);
            if (match == null) return;

            Editor.OpenForEdit(match.Id, match.Number);
        }

        private async Task OnDeleteTableAsync(TableViewModel? table)
        {
            if (table == null) return;

            // Block if table has an active order
            if (_orderStore.HasOrder(table.tableNumber))
            {
                System.Windows.MessageBox.Show(
                    $"Table {table.tableNumber} has an active order.\n" +
                    "Please close or transfer the order before deleting this table.",
                    "Cannot Delete",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"Remove Table {table.tableNumber}?\n\nThis hides it from the system. Historical orders are preserved.",
                "Remove Table",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            try
            {
                var tables = await _tableService.GetAllAsync();
                var match = tables.FirstOrDefault(t => t.Number == table.tableNumber);
                if (match == null) return;

                await _tableService.DeleteTableAsync(match.Id);
                await ReloadTablesAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.Message, "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }

}
