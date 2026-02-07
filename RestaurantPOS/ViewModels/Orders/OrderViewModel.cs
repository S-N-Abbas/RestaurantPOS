using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
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

        public bool CanPay =>
            _orderState?.Order != null &&
            OrderItems.Any();

        public ICommand OpenTablePickerCommand { get; }
        public ICommand CloseTablePickerCommand { get; }

        public TablePickerViewModel TablePicker { get; }


        public decimal GrandTotal => OrderItems.Sum(i => i.Total);

        public ICommand AddItemCommand { get; }
        public ICommand ChangeTableCommand { get; }
        public IRelayCommand PayCommand { get; }

        private readonly ITableSessionService _tableSession;
        private readonly OrderStore _orderStore;
        private OrderState _orderState;
        private readonly OrderService _orderService;
        private readonly INavigationService _navigationService;
        private readonly TableStore _tableStore;

        private int _tableNumber;
        public int TableNumber
        {
            get => _tableNumber;
            private set => SetProperty(ref _tableNumber, value);
        }

        private readonly IMenuDataService _menuService;

        private void RaisePayState()
        {
            OnPropertyChanged(nameof(CanPay));
        }

        public OrderViewModel(
            ITableSessionService tableSession,
            OrderStore orderStore,
            IMenuDataService menuService,
            OrderService orderService,
            INavigationService navigationService,
            TableStore tableStore)
        {
            _menuService = menuService;
            _tableSession = tableSession;
            _orderStore = orderStore;
            _orderService = orderService;
            _navigationService = navigationService;


            Categories = new();
            AllItems = new();
            Items = new();

            // Comands
            AddItemCommand = new RelayCommand<MenuItemViewModel>(AddItem);

            ChangeTableCommand = new RelayCommand(() =>
            {
                // TEMP: cycle tables for now
                var next = TableNumber == 5 ? 1 : TableNumber + 1;
                _tableSession.SwitchTable(next);
            });

            PayCommand = new AsyncRelayCommand(PayAsync);

            OrderItems = new ObservableCollection<OrderItemViewModel>();
            
            OrderItems.CollectionChanged += (_, __) =>
                UpdateOrderState();

            SelectedCategory = Categories.FirstOrDefault();

            _ = LoadMenuAsync();

            _tableSession.TableChanged += OnTableChanged;

            TablePicker = new TablePickerViewModel(tableSession, orderStore, tableStore);

            OpenTablePickerCommand = new RelayCommand(() => IsTablePickerOpen = true);
            CloseTablePickerCommand = new RelayCommand(() => IsTablePickerOpen = false);

            _tableSession.TableChanged += _ => IsTablePickerOpen = false;

            LoadTable(_tableSession.CurrentTable);
        }

        private void UpdateOrderState()
        {
            OnPropertyChanged(nameof(GrandTotal));
            OnPropertyChanged(nameof(CanPay));
            PayCommand.NotifyCanExecuteChanged();
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
            UpdateOrderState();
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
                    item.PropertyChanged += OrderItem_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (OrderItemViewModel item in e.OldItems)
                    item.PropertyChanged -= OrderItem_PropertyChanged;
            }

            UpdateOrderState();
        }

        private async void OrderItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not OrderItemViewModel item)
                return;

            if (e.PropertyName == nameof(OrderItemViewModel.Quantity) ||
                e.PropertyName == nameof(OrderItemViewModel.Total))
            {
                UpdateOrderState();

                if (_orderState.Order == null)
                    return;

                await _orderService.UpdateQuantityAsync(
                    _orderState.Order,
                    item.ItemId,
                    item.Quantity);
            }
        }


        private async void AddItem(MenuItemViewModel item)
        {
            if (_orderState.Order == null)
            {
                var order = await _orderService.CreateOrderAsync(TableNumber);
                _orderState.AttachOrder(order);
                UpdateOrderState();
            }

            await _orderService.AddItemAsync(_orderState.Order!, item);

            var existing = OrderItems.FirstOrDefault(i => i.ItemId == item.Id);

            if (existing != null)
                existing.Quantity++;
            else
                OrderItems.Add(new OrderItemViewModel(item, RemoveItem));
        }

        private async void RemoveItem(OrderItemViewModel item)
        {
            OrderItems.Remove(item);
            if (_orderState.Order != null)
            {
                await _orderService.UpdateQuantityAsync(
                    _orderState.Order,
                    item.ItemId,
                    0);
            }
            UpdateOrderState();
        }

        private async Task PayAsync()
        {
            if (_orderState.Order == null)
                return;

            await _orderService.CloseOrderAsync(_orderState.Order);

            _orderStore.CloseOrder(TableNumber);

            _navigationService.NavigateTo<TablesViewModel>();
        }

        private async void Pay()
        {
            if (_orderState.Order == null)
                return;

            await _orderService.CloseOrderAsync(_orderState.Order);

            _orderStore.CloseOrder(TableNumber);

            _navigationService.NavigateTo<TablesViewModel>();
        }

    }
}
