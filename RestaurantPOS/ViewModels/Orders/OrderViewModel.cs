using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Cover;
using RestaurantPOS.ViewModels.Payments;
using RestaurantPOS.ViewModels.Tables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace RestaurantPOS.ViewModels.Orders
{
    public class OrderViewModel : ViewModelBase
    {
        private readonly IOrderContextService _orderContextService;
        private readonly OrderStore _orderStore;
        private OrderState _orderState;
        private readonly OrderService _orderService;
        private readonly INavigationService _navigationService;
        private readonly TableStore _tableStore;
        private readonly SettingsService _settingsService;
        private readonly AuthorizationService _authorizationService;
        private readonly IMenuAdminService _menuAdmin;

        /// <summary>True for Admin and Manager — drives + button visibility in XAML.</summary>
        public bool CanEditMenu => _authorizationService
            .HasAccess(UserRole.Admin, UserRole.Manager);

        public InlineMenuEditorViewModel MenuEditor { get; }

        public OpenItemEditorViewModel OpenItemEditor { get; }
        public ICommand OpenItemCommand { get; }

        public CoverSelectorViewModel coverSelectorViewModel { get; set; }

        public ObservableCollection<CategoryViewModel> Categories { get; }
        public ObservableCollection<MenuItemViewModel> AllItems { get; }
        public ObservableCollection<MenuItemViewModel> Items { get; }
        public ObservableCollection<OrderItemViewModel> OrderItems { get; private set; }

        // ─── Transfer Table ─────────────────────────────────────────────────────────────
        public bool IsTableTransferOpen
        {
            get => _isTableTransferOpen;
            set => SetProperty(ref _isTableTransferOpen, value);
        }
        private bool _isTableTransferOpen;

        // Only show for DineIn orders that have an actual order record
        public bool CanTransferTable =>
            _orderContextService.CurrentOrderType == OrderType.DineIn
            && _orderState?.Order != null;

        public TableTransferViewModel? TableTransfer { get; private set; }

        public ICommand OpenTableTransferCommand { get; }
        public ICommand CloseTableTransferCommand { get; }

        private CategoryViewModel? _selectedCategory;
        public CategoryViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    FilterItems();
            }
        }

        private bool _isOrderSwitcherOpen;
        public bool IsOrderSwitcherOpen
        {
            get => _isOrderSwitcherOpen;
            set => SetProperty(ref _isOrderSwitcherOpen, value);
        }


        private bool _isCoverSelectorOpen;
        public bool IsCoverSelectorOpen
        {
            get => _isCoverSelectorOpen;
            set
            {
                _isCoverSelectorOpen = value;
                OnPropertyChanged();
            }
        }

        public string CoversDisplay =>
            $"Adults: {_orderState.Order?.AdultCovers ?? 0}   •   Children: {_orderState.Order?.ChildCovers ?? 0}";

        public bool IsDineIn => _orderContextService.CurrentOrderType == OrderType.DineIn;

        // Expose the symbol for binding
        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        public ICommand OpenOrderSwitcherCommand { get; }
        public ICommand CloseOrderSwitcherCommand { get; }

        public ICommand OpenCoverSelectorCommand { get; }
        public ICommand CloseCoverSelectorCommand { get; }

        public ICommand CancelOrderCommand { get; }

        public ICommand AddCategoryCommand { get; }
        public ICommand AddProductCommand { get; }

        public bool CanCancel => _orderState?.Order != null;
        public OrderSwitcherViewModel OrderSwitcher { get; }

        public decimal CoversTotal =>
            _orderState.Order == null ? 0 :
            _settingsService.CalculateCoverCharge(_orderState.Order);
        public bool CanPay => _orderState?.Order != null && OrderItems.Any();
        public decimal ItemsTotal =>
            OrderItems.Sum(i => i.Total);
        public decimal GrandTotal => 
            CoversTotal + OrderItems.Sum(i => i.Total);


        // ─── Edit Mode ────────────────────────────────────────────────────────────────

        private bool _isEditMenuMode;
        public bool IsEditMenuMode
        {
            get => _isEditMenuMode;
            set
            {
                if (SetProperty(ref _isEditMenuMode, value))
                    OnPropertyChanged(nameof(EditMenuButtonLabel));
            }
        }

        public string EditMenuButtonLabel
            => IsEditMenuMode ? "Done Editing" : "Edit Menu";

        public ICommand ToggleEditMenuCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ICommand AddItemCommand { get; }
        public ICommand PayCommand { get; }

        private int _tableNumber;
        public int TableNumber
        {
            get => _tableNumber;
            private set => SetProperty(ref _tableNumber, value);
        }

        private readonly IMenuDataService _menuService;

        public OrderViewModel(
            IOrderContextService orderContextService,
            OrderStore orderStore,
            IMenuDataService menuService,
            OrderService orderService,
            INavigationService navigationService,
            TableStore tableStore,
            SettingsService settingsService,
            AuthorizationService authorizationService,
            IMenuAdminService menuAdmin)
        {
            _menuService = menuService;
            _orderContextService = orderContextService;
            _orderStore = orderStore;
            _orderService = orderService;
            _tableStore = tableStore;
            _navigationService = navigationService;
            _settingsService = settingsService;
            _authorizationService = authorizationService;
            _menuAdmin = menuAdmin;

            MenuEditor = new InlineMenuEditorViewModel(menuAdmin, _settingsService);

            OpenItemEditor = new OpenItemEditorViewModel(_settingsService);

            OpenItemEditor.Confirmed += async (name, price) =>
                await AddOpenItemAsync(name, price);

            OpenItemCommand = new RelayCommand(() => OpenItemEditor.Open());

            ToggleEditMenuCommand = new RelayCommand(
                () => IsEditMenuMode = !IsEditMenuMode,
                () => CanEditMenu);

            EditCategoryCommand = new RelayCommand<CategoryViewModel>(
                async c => await EditCategoryAsync(c));

            DeleteCategoryCommand = new RelayCommand<CategoryViewModel>(
                async c => await DeleteCategoryAsync(c));

            EditProductCommand = new RelayCommand<MenuItemViewModel>(
                async p => await EditProductAsync(p));

            DeleteProductCommand = new RelayCommand<MenuItemViewModel>(
                async p => await DeleteProductAsync(p));

            // After save, reload menu live and close editor
            MenuEditor.SavedSuccessfully += async _ =>
            {
                await ReloadMenuAsync();
            };

            AddCategoryCommand = new RelayCommand(() =>
                MenuEditor.OpenForCategory());

            AddProductCommand = new RelayCommand(() =>
                MenuEditor.OpenForProduct(Categories, SelectedCategory));

            CancelOrderCommand = new RelayCommand(
                () => _ = CancelOrderAsync(),
                () => CanCancel
            );

            Categories = new();
            AllItems = new();
            Items = new();

            // Comands
            AddItemCommand = new RelayCommand<MenuItemViewModel>(
                item => _ = AddItemAsync(item)
            );

            PayCommand = new RelayCommand(
                ExecutePay,
                () => _orderState?.Order != null && OrderItems.Any()
            );

            OpenTableTransferCommand = new RelayCommand(
                OpenTableTransfer,
                () => CanTransferTable);

            CloseTableTransferCommand = new RelayCommand(
                () => IsTableTransferOpen = false);

            OrderItems = new ObservableCollection<OrderItemViewModel>();
            
            OrderItems.CollectionChanged += (_, __) =>
                UpdateOrderState();

            SelectedCategory = Categories.FirstOrDefault();

            _ = LoadMenuAsync();

            _orderContextService.ContextChanged += OnTableChanged;

            OrderSwitcher = new OrderSwitcherViewModel(_orderContextService, _orderStore, _tableStore, _settingsService);

            OpenOrderSwitcherCommand = new RelayCommand(() => IsOrderSwitcherOpen = true);
            CloseOrderSwitcherCommand = new RelayCommand(() => IsOrderSwitcherOpen = false);

            OpenCoverSelectorCommand = new RelayCommand(() => IsCoverSelectorOpen = true);
            CloseCoverSelectorCommand = new RelayCommand(() => IsCoverSelectorOpen = false);

            _orderContextService.ContextChanged += _ => IsOrderSwitcherOpen = false;

            LoadTable(_orderContextService.CurrentContext);

            coverSelectorViewModel = new CoverSelectorViewModel(
                _orderState,
                _orderService,
                _orderContextService,
                _settingsService);

            coverSelectorViewModel.RequestClose += CloseCoverSelector;

            // Ensure the UI updates if settings change
            _settingsService.SettingsChanged += () => OnPropertyChanged(nameof(CurrencySymbol));
        }

        private void UpdateOrderState()
        {
            if (coverSelectorViewModel != null)
                coverSelectorViewModel.Reload(_orderState);

            if (_orderContextService.CurrentOrderType == OrderType.DineIn)
            {
                if (_orderState.Order == null ||
                    (_orderState.Order.AdultCovers == 0 && _orderState.Order.ChildCovers == 0))
                    IsCoverSelectorOpen = true;
            }
            else
            {
                IsCoverSelectorOpen = false;
            }

            OnPropertyChanged(nameof(CoversDisplay));
            OnPropertyChanged(nameof(GrandTotal));
            OnPropertyChanged(nameof(CanPay));
            OnPropertyChanged(nameof(CanCancel));
            ((RelayCommand)PayCommand).NotifyCanExecuteChanged();
            ((RelayCommand)CancelOrderCommand).NotifyCanExecuteChanged();
            
            OnPropertyChanged(nameof(CanTransferTable));
            ((RelayCommand)OpenTableTransferCommand).NotifyCanExecuteChanged();
        }

        private async void LoadTable(int tableNumber)
        {
            TableNumber = tableNumber;

            if (_orderState != null)
                _orderState.Items.CollectionChanged -= OrderItems_CollectionChanged;

            _orderState = await _orderStore.GetOrCreateAsync(
                tableNumber,
                RemoveItem);

            OrderItems = _orderState.Items;
            OnPropertyChanged(nameof(OrderItems));

            OrderItems.CollectionChanged += OrderItems_CollectionChanged;

            // hook existing items
            foreach (var item in OrderItems)
            {
                item.PropertyChanged += OrderItem_PropertyChanged;
            }

            UpdateOrderState();            
        }

        private void CloseCoverSelector()
        {
            IsCoverSelectorOpen = false;
            UpdateOrderState();
        }


        private void OnTableChanged(int contextId)
        {
            LoadTable(contextId);
            OnPropertyChanged(nameof(ContextLabel));
            OnPropertyChanged(nameof(ContextIcon));
            OnPropertyChanged(nameof(IsDineIn));
            OnPropertyChanged(nameof(CanTransferTable));
        }


        public string ContextLabel => _orderContextService.CurrentOrderType switch
        {
            OrderType.TakeAway => $"TAKEAWAY #{Math.Abs(_orderContextService.CurrentContext)}",
            OrderType.Delivery => $"DELIVERY #{Math.Abs(_orderContextService.CurrentContext)}",
            _ => $"TABLE {_orderContextService.CurrentContext}"
        };

        // MahApps icon kind as a string — WPF resolves it via the enum converter
        public string ContextIcon => _orderContextService.CurrentOrderType switch
        {
            OrderType.TakeAway => "BagPersonal",
            OrderType.Delivery => "Moped",
            _ => "TableChair"
        };

        private async Task LoadMenuAsync()
        {
            var categories = await _menuService.GetCategoriesAsync();
            var products = await _menuService.GetProductsAsync();

            Categories.Clear();
            foreach (var c in categories)
                Categories.Add(new CategoryViewModel(c.Id, c.Name));

            AllItems.Clear();
            foreach (var p in products)
                AllItems.Add(new MenuItemViewModel(p.Id, p.Name, p.Price, p.CategoryId, _settingsService));

            SelectedCategory = Categories.FirstOrDefault();
            FilterItems();
        }

        /// <summary>
        /// Called after an inline save. Reloads categories and products live
        /// without navigating away. MenuDataService cache was already invalidated
        /// by MenuAdminService before this fires.
        /// </summary>
        private async Task ReloadMenuAsync()
        {
            await LoadMenuAsync();  // cache was busted by MenuAdminService — fresh DB read
        }

        private void FilterItems()
        {
            Items.Clear();

            if (SelectedCategory == null)
                return;

            foreach (var item in AllItems.Where(i => i.CategoryId == SelectedCategory.Id))
                Items.Add(item);
        }

        private void OrderItems_CollectionChanged(object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (OrderItemViewModel item in e.NewItems)
                {
                    item.PropertyChanged -= OrderItem_PropertyChanged;
                    item.PropertyChanged += OrderItem_PropertyChanged;
                }
            }
            UpdateOrderState();
        }

        private void OrderItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not OrderItemViewModel item)
                return;

            if (e.PropertyName == nameof(OrderItemViewModel.Quantity))
            {
                _ = HandleQuantityChangedAsync(item);
            }
        }

        private async Task HandleQuantityChangedAsync(OrderItemViewModel item)
        {
            UpdateOrderState();

            if (_orderState.Order == null)
                return;

            // open item
            if(item.ItemId == 0)
            {
                await _orderService.UpdateOpenItemQuantityAsync(_orderState.Order.Id, item.OrderItemId, item.Quantity);
            }
            else
            {
                await _orderService.UpdateQuantityAsync(
                    _orderState.Order.Id,
                    item.ItemId,
                    item.Quantity);
            }


            var updatedOrder = await _orderService.GetByIdAsync(_orderState.Order.Id);
            _orderState.UpdateFrom(updatedOrder);
        }


        private async Task AddItemAsync(MenuItemViewModel item)
        {
            if (_orderState.Order == null)
            {
                var order = await _orderService.CreateOrderAsync(
                    TableNumber,
                    _orderContextService.CurrentOrderType); // ✅ pass correct OrderType

                _orderState.AttachOrder(order);
                _orderStore.NotifyOrderStateChanged(TableNumber); // ✅ tell switcher a real order now exists
            }

            await _orderService.AddItemAsync(_orderState.Order!.Id, item.Id);

            var existing = OrderItems.FirstOrDefault(i => i.ItemId == item.Id);

            if (existing != null)
                existing.Quantity++;
            else
                OrderItems.Add(new OrderItemViewModel(item, _settingsService, RemoveItem));

            UpdateOrderState();
            var updatedOrder = await _orderService.GetByIdAsync(_orderState.Order.Id);
            _orderState.UpdateFrom(updatedOrder);
        }


        private void RemoveItem(OrderItemViewModel item)
        {
            _ = RemoveItemAsync(item);
        }

        private async Task RemoveItemAsync(OrderItemViewModel item)
        {
            OrderItems.Remove(item);

            if (_orderState.Order != null)
            {
                if (item.ItemId == 0)
                {
                    // ✅ Open item — delete by OrderItem.Id
                    await _orderService.RemoveOpenItemAsync(
                        _orderState.Order.Id,
                        item.OrderItemId);
                }
                else
                {
                    // Regular menu item — set quantity to 0
                    await _orderService.UpdateQuantityAsync(
                        _orderState.Order.Id,
                        item.ItemId,
                        0);
                }
            }

            UpdateOrderState();

            if (_orderState.Order == null)
                throw new InvalidOperationException("Order should not be null here");

            var updatedOrder = await _orderService.GetByIdAsync(_orderState.Order.Id);
            _orderState.UpdateFrom(updatedOrder);
        }

        private void ExecutePay()
        {
            _navigationService.NavigateTo<PaymentViewModel>(_orderState);
        }

        private async Task CancelOrderAsync()
        {
            if (_orderState.Order == null)
                return;

            // ── Guard: payments already recorded requires manager ──────────────────
            bool hasPaidAmount = _orderState.Order.PaidAmount > 0;

            if (hasPaidAmount)
            {
                bool isManager = _authorizationService
                    .HasAccess(UserRole.Manager, UserRole.Admin);

                if (!isManager)
                {
                    MessageBox.Show(
                        "This order has payments recorded.\nA Manager or Admin is required to cancel it.",
                        "Authorisation Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            // ── Confirmation dialog ─────────────────────────────────────────────────
            string summary = $"Order #{_orderState.Order.Id}  •  {OrderItems.Count} item(s)  •  {CurrencySymbol}{GrandTotal:N2}";

            var result = MessageBox.Show(
                $"Are you sure you want to cancel this order?\n\n{summary}\n\nThis cannot be undone.",
                "Cancel Order",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // ── Mark as cancelled in DB ─────────────────────────────────────────
                await _orderService.CancelOrderAsync(_orderState.Order.Id);

                // ── Remove from in-memory store ─────────────────────────────────────
                _orderStore.CloseOrder(_orderState.ContextId);

                // ── Navigate back to tables ─────────────────────────────────────────
                _navigationService.NavigateTo<TablesViewModel>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to cancel order: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task EditCategoryAsync(CategoryViewModel? category)
        {
            if (category == null) return;
            MenuEditor.OpenForEditCategory(category);
        }

        private async Task DeleteCategoryAsync(CategoryViewModel? category)
        {
            if (category == null) return;

            var result = MessageBox.Show(
                $"Delete category '{category.Name}'?\n\nAll products in this category will be hidden.",
                "Delete Category",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _menuAdmin.DeleteCategoryAsync(category.Id);
                await ReloadMenuAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EditProductAsync(MenuItemViewModel? product)
        {
            if (product == null) return;
            MenuEditor.OpenForEditProduct(product, Categories, SelectedCategory);
        }

        private async Task DeleteProductAsync(MenuItemViewModel? product)
        {
            if (product == null) return;

            var result = MessageBox.Show(
                $"Delete '{product.Name}'?",
                "Delete Product",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _menuAdmin.DeleteProductAsync(product.Id);
                await ReloadMenuAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenTableTransfer()
        {
            // Build fresh each time so occupied state is current
            TableTransfer = new TableTransferViewModel(
                _orderContextService.CurrentContext,
                _orderStore,
                _tableStore);

            TableTransfer.TransferRequested += OnTransferRequested;
            TableTransfer.Cancelled += () => IsTableTransferOpen = false;

            OnPropertyChanged(nameof(TableTransfer));
            IsTableTransferOpen = true;
        }

        private async void OnTransferRequested(int destinationTableNumber)
        {
            if (_orderState.Order == null) return;

            try
            {
                int fromContextId = _orderContextService.CurrentContext;

                // ── 1. Persist to DB ──────────────────────────────────────────────
                await _orderService.TransferTableAsync(
                    _orderState.Order.Id,
                    destinationTableNumber);

                // ── 2. Update in-memory store ─────────────────────────────────────
                _orderStore.TransferOrder(fromContextId, destinationTableNumber);

                // ── 3. Close the picker ───────────────────────────────────────────
                IsTableTransferOpen = false;

                // ── 4. Switch context to the new table — OrderViewModel reloads ──
                _orderContextService.SwitchContext(destinationTableNumber);
            }
            catch (Exception ex)
            {
                IsTableTransferOpen = false;
                MessageBox.Show(
                    $"Transfer failed: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task AddOpenItemAsync(string name, decimal price)
        {
            if (_orderState.Order == null)
            {
                var order = await _orderService.CreateOrderAsync(
                    TableNumber,
                    _orderContextService.CurrentOrderType);
                _orderState.AttachOrder(order);
                _orderStore.NotifyOrderStateChanged(TableNumber);
            }

            // ✅ capture the returned item so we have its DB Id
            var savedItem = await _orderService.AddOpenItemAsync(
                _orderState.Order!.Id, name, price);

            OrderItems.Add(new OrderItemViewModel(
                itemId: 0,
                orderItemId: savedItem.Id,    // ✅ pass DB row Id
                name: name,
                unitPrice: price,
                settingsService: _settingsService,
                removeCallback: RemoveItem));

            UpdateOrderState();

            var updatedOrder = await _orderService.GetByIdAsync(_orderState.Order.Id);
            _orderState.UpdateFrom(updatedOrder);
        }
    }
}
