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
using System.Windows.Input;
using System.Windows.Navigation;

namespace RestaurantPOS.ViewModels.Orders
{
    public class OrderViewModel : ViewModelBase
    {
        public CoverSelectorViewModel coverSelectorViewModel { get; }

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

        private bool _isTablePickerOpen;
        public bool IsTablePickerOpen
        {
            get => _isTablePickerOpen;
            set => SetProperty(ref _isTablePickerOpen, value);
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


        public ICommand OpenTablePickerCommand { get; }
        public ICommand CloseTablePickerCommand { get; }

        public ICommand OpenCoverSelectorCommand { get; }
        public ICommand CloseCoverSelectorCommand { get; }

        public TablePickerViewModel TablePicker { get; }

        public decimal CoversTotal =>
            _orderState.Order == null ? 0 :
            _pricing.CalculateCoverCharge(_orderState.Order);
        public bool CanPay => _orderState?.Order != null && OrderItems.Any();
        public decimal ItemsTotal =>
            OrderItems.Sum(i => i.Total);
        public decimal GrandTotal => 
            CoversTotal + OrderItems.Sum(i => i.Total);

        public ICommand AddItemCommand { get; }
        public ICommand PayCommand { get; }

        private readonly ITableSessionService _tableSession;
        private readonly OrderStore _orderStore;
        private OrderState _orderState;
        private readonly OrderService _orderService;
        private readonly INavigationService _navigationService;
        private readonly TableStore _tableStore;
        private readonly IPricingService _pricing;

        private int _tableNumber;
        public int TableNumber
        {
            get => _tableNumber;
            private set => SetProperty(ref _tableNumber, value);
        }

        private readonly IMenuDataService _menuService;

        public OrderViewModel(
            ITableSessionService tableSession,
            OrderStore orderStore,
            IMenuDataService menuService,
            OrderService orderService,
            INavigationService navigationService,
            TableStore tableStore,
            IPricingService pricing)
        {
            _menuService = menuService;
            _tableSession = tableSession;
            _orderStore = orderStore;
            _orderService = orderService;
            _navigationService = navigationService;
            _pricing = pricing;

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

            _tableSession.TableChanged += OnTableChanged;

            TablePicker = new TablePickerViewModel(tableSession, orderStore, tableStore);

            OpenTablePickerCommand = new RelayCommand(() => IsTablePickerOpen = true);
            CloseTablePickerCommand = new RelayCommand(() => IsTablePickerOpen = false);

            OpenCoverSelectorCommand = new RelayCommand(() => IsCoverSelectorOpen = true);
            CloseCoverSelectorCommand = new RelayCommand(() => IsCoverSelectorOpen = false);

            _tableSession.TableChanged += _ => IsTablePickerOpen = false;

            LoadTable(_tableSession.CurrentTable);

            coverSelectorViewModel = new CoverSelectorViewModel(
                _orderState,
                _orderService);

            coverSelectorViewModel.RequestClose += CloseCoverSelector;

            if (_orderState.Order!.AdultCovers == 0 && _orderState.Order!.ChildCovers == 0)
                IsCoverSelectorOpen = true;

        }

        private void UpdateOrderState()
        {
            OnPropertyChanged(nameof(GrandTotal));
            OnPropertyChanged(nameof(CanPay));
            ((RelayCommand)PayCommand).NotifyCanExecuteChanged();
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
        }


        private void OnTableChanged(int tableNumber)
        {
            LoadTable(tableNumber);
        }


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
                var order = await _orderService.CreateOrderAsync(TableNumber);
                _orderState.AttachOrder(order);
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
            var updatedOrder = await _orderService.GetByIdAsync(_orderState.Order.Id);
            _orderState.UpdateFrom(updatedOrder);
        }

        private void ExecutePay()
        {
            _navigationService.NavigateTo<PaymentViewModel>(_orderState);
        }
    }
}
