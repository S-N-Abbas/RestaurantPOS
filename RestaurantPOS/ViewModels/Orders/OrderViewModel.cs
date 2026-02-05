using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Orders
{
    public class OrderViewModel : ViewModelBase
    {
        public int TableNumber { get; }

        public ObservableCollection<CategoryViewModel> Categories { get; }
        public ObservableCollection<MenuItemViewModel> AllItems { get; }
        public ObservableCollection<MenuItemViewModel> Items { get; }
        public ObservableCollection<OrderItemViewModel> OrderItems { get; }

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

        public decimal GrandTotal => OrderItems.Sum(i => i.Total);

        public ICommand AddItemCommand { get; }

        public OrderViewModel(int tableNumber)
        {
            TableNumber = tableNumber;

            Categories = new();
            AllItems = new();
            Items = new();
            OrderItems = new();

            AddItemCommand = new RelayCommand<MenuItemViewModel>(AddItem);

            LoadMockData();
            SelectedCategory = Categories.FirstOrDefault();

            OrderItems.CollectionChanged += (_, __) =>
                OnPropertyChanged(nameof(GrandTotal));
        }

        private void LoadMockData()
        {
            Categories.Add(new(1, "Drinks"));
            Categories.Add(new(2, "Food"));

            AllItems.Add(new(1, "Tea", 150, 1));
            AllItems.Add(new(2, "Coffee", 200, 1));
            AllItems.Add(new(3, "Burger", 550, 2));
            AllItems.Add(new(4, "Pizza", 900, 2));
        }

        private void FilterItems()
        {
            Items.Clear();

            if (SelectedCategory == null)
                return;

            foreach (var item in AllItems.Where(i => i.CategoryId == SelectedCategory.Id))
                Items.Add(item);
        }

        private void AddItem(MenuItemViewModel? item)
        {
            if (item == null) return;

            var existing = OrderItems.FirstOrDefault(i => i.ItemId == item.Id);

            if (existing != null)
                existing.Quantity++;
            else
                OrderItems.Add(new OrderItemViewModel(item));

            OnPropertyChanged(nameof(GrandTotal));
        }
    }
}
