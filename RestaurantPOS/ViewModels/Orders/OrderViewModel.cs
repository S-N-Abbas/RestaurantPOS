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

        public CoverSelectorViewModel coverSelectorViewModel { get; set; }

        public ObservableCollection<CategoryViewModel> Categories { get; }
        public ObservableCollection<MenuItemViewModel> AllItems { get; }
        public ObservableCollection<MenuItemViewModel> Items { get; }
        public ObservableCollection<OrderItemViewModel> OrderItems { get; private set; }

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
            _navigationService = navigationService;
            _settingsService = settingsService;
            _authorizationService = authorizationService;
            _menuAdmin = menuAdmin;

            MenuEditor = new InlineMenuEditorViewModel(menuAdmin);

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

            OrderItems = new ObservableCollection<OrderItemViewModel>();
            
            OrderItems.CollectionChanged += (_, __) =>
                UpdateOrderState();

            SelectedCategory = Categories.FirstOrDefault();

            _ = LoadMenuAsync();

            _orderContextService.ContextChanged += OnTableChanged;

            OrderSwitcher = new OrderSwitcherViewModel(orderContextService, orderStore, tableStore);

            OpenOrderSwitcherCommand = new RelayCommand(() => IsOrderSwitcherOpen = true);
            CloseOrderSwitcherCommand = new RelayCommand(() => IsOrderSwitcherOpen = false);

            OpenCoverSelectorCommand = new RelayCommand(() => IsCoverSelectorOpen = true);
            CloseCoverSelectorCommand = new RelayCommand(() => IsCoverSelectorOpen = false);

            _orderContextService.ContextChanged += _ => IsOrderSwitcherOpen = false;

            LoadTable(_orderContextService.CurrentContext);

            coverSelectorViewModel = new CoverSelectorViewModel(
                _orderState,
                _orderService,
                _orderContextService);

            coverSelectorViewModel.RequestClose += CloseCoverSelector;
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
                AllItems.Add(new MenuItemViewModel(p.Id, p.Name, p.Price, p.CategoryId));

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

            await _orderService.UpdateQuantityAsync(
                _orderState.Order.Id,
                item.ItemId,
                item.Quantity);

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
                OrderItems.Add(new OrderItemViewModel(item, RemoveItem));

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
                await _orderService.UpdateQuantityAsync(
                    _orderState.Order.Id,
                    item.ItemId,
                    0);
            }

            UpdateOrderState();

            if(_orderState.Order == null)
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
            string summary = $"Order #{_orderState.Order.Id}  •  {OrderItems.Count} item(s)  •  £{GrandTotal:N2}";

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
    }
}
