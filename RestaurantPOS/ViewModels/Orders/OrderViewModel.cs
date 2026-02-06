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

        public ICommand OpenTablePickerCommand { get; }
        public ICommand CloseTablePickerCommand { get; }

        public TablePickerViewModel TablePicker { get; }


        public decimal GrandTotal => OrderItems.Sum(i => i.Total);

        public ICommand AddItemCommand { get; }
        public ICommand ChangeTableCommand { get; }

        private readonly ITableSessionService _tableSession;
        private readonly OrderStore _orderStore;
        private OrderState _orderState;

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
            IMenuDataService menuService)
        {
            _menuService = menuService;
            _tableSession = tableSession;
            _orderStore = orderStore;

            Categories = new();
            AllItems = new();
            Items = new();

            OrderItems = new ObservableCollection<OrderItemViewModel>();
            OrderItems.CollectionChanged += (_, __) =>
           OnPropertyChanged(nameof(GrandTotal));

            SelectedCategory = Categories.FirstOrDefault();

            _ = LoadMenuAsync();

            _tableSession.TableChanged += OnTableChanged;

            TablePicker = new TablePickerViewModel(tableSession, orderStore);

            OpenTablePickerCommand = new RelayCommand(() => IsTablePickerOpen = true);
            CloseTablePickerCommand = new RelayCommand(() => IsTablePickerOpen = false);

            _tableSession.TableChanged += _ => IsTablePickerOpen = false;

            LoadTable(_tableSession.CurrentTable);

            AddItemCommand = new RelayCommand<MenuItemViewModel>(AddItem);

            ChangeTableCommand = new RelayCommand(() =>
            {
                // TEMP: cycle tables for now
                var next = TableNumber == 5 ? 1 : TableNumber + 1;
                _tableSession.SwitchTable(next);
            });
        }

        private void LoadTable(int tableNumber)
        {
            TableNumber = tableNumber;

            if (_orderState != null)
                _orderState.Items.CollectionChanged -= OrderItems_CollectionChanged;

            _orderState = _orderStore.GetOrCreate(tableNumber);

            OrderItems = _orderState.Items;
            OnPropertyChanged(nameof(OrderItems));

            OrderItems.CollectionChanged += OrderItems_CollectionChanged;

            OnPropertyChanged(nameof(GrandTotal));
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

            OnPropertyChanged(nameof(GrandTotal));
        }

        private void OrderItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderItemViewModel.Quantity) ||
                e.PropertyName == nameof(OrderItemViewModel.Total))
            {
                OnPropertyChanged(nameof(GrandTotal));
            }
        }


        private void AddItem(MenuItemViewModel item)
        {
            var existing = OrderItems.FirstOrDefault(i => i.ItemId == item.Id);

            if (existing != null)
                existing.Quantity++;
            else
                OrderItems.Add(new OrderItemViewModel(item, RemoveItem));

            OnPropertyChanged(nameof(GrandTotal));
        }

        private void RemoveItem(OrderItemViewModel item)
        {
            OrderItems.Remove(item);
            OnPropertyChanged(nameof(GrandTotal));
        }
    }
}
