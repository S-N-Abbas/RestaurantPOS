using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Orders
{
    
    public enum InlineEditorMode
    {
        Category,
        Product
    }

    public enum InlineEditorField
    {
        Name,
        Price
    }

    public class InlineMenuEditorViewModel : ViewModelBase
    {
        private int? _editingId;
        private readonly IMenuAdminService _menuAdmin;

        // ─── Mode ─────────────────────────────────────────────────────────────

        private InlineEditorMode _mode;
        public InlineEditorMode Mode
        {
            get => _mode;
            private set
            {
                if (SetProperty(ref _mode, value))
                {
                    OnPropertyChanged(nameof(IsCategoryMode));
                    OnPropertyChanged(nameof(IsProductMode));
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public bool IsCategoryMode => Mode == InlineEditorMode.Category;
        public bool IsProductMode => Mode == InlineEditorMode.Product;

        public string Title => Mode switch
        {
            InlineEditorMode.Category => _editingId == null ? "New Category" : "Edit Category",
            _ => _editingId == null ? "New Product" : "Edit Product"
        };

        // ─── Fields ───────────────────────────────────────────────────────────

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _price = string.Empty;
        public string Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        // Which field the on-screen keyboard targets
        private InlineEditorField _activeField = InlineEditorField.Name;

        public bool IsNameActive => _activeField == InlineEditorField.Name;
        public bool IsPriceActive => _activeField == InlineEditorField.Price;

        // Category list for the product category picker
        public ObservableCollection<CategoryViewModel> Categories { get; } = new();

        private CategoryViewModel? _selectedCategory;
        public CategoryViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        // ─── Visibility ───────────────────────────────────────────────────────

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        // ─── Events ───────────────────────────────────────────────────────────

        /// <summary>Fired after a successful save. Payload is the saved entity.</summary>
        public event Action<object>? SavedSuccessfully;

        public event Action? Cancelled;

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand SelectCategoryCommand { get; }
        public ICommand FocusNameCommand { get; }
        public ICommand FocusPriceCommand { get; }
        public ICommand KeyCommand { get; }   // on-screen keyboard key
        public ICommand BackspaceCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public InlineMenuEditorViewModel(IMenuAdminService menuAdmin)
        {
            _menuAdmin = menuAdmin;

            SelectCategoryCommand = new RelayCommand<CategoryViewModel>(cat =>
            {
                SelectedCategory = cat;
                // Refresh all chips so highlight updates
                foreach (var c in Categories)
                    c.RaiseIsSelected(SelectedCategory?.Id);
            });

            FocusNameCommand = new RelayCommand(() => SetActiveField(InlineEditorField.Name));
            FocusPriceCommand = new RelayCommand(() => SetActiveField(InlineEditorField.Price));
            KeyCommand = new RelayCommand<string>(AppendKey);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearCommand = new RelayCommand(ClearActiveField);
            SaveCommand = new RelayCommand(async () => await SaveAsync());
            CancelCommand = new RelayCommand(Close);
        }

        // ─── Open API ─────────────────────────────────────────────────────────

        public void OpenForCategory()
        {
            Mode = InlineEditorMode.Category;
            Name = string.Empty;
            Price = string.Empty;
            _activeField = InlineEditorField.Name;
            RaiseFieldFocus();
            IsOpen = true;
        }

        public void OpenForProduct(IEnumerable<CategoryViewModel> categories,
                                   CategoryViewModel? defaultCategory)
        {
            Mode = InlineEditorMode.Product;
            Name = string.Empty;
            Price = string.Empty;
            _activeField = InlineEditorField.Name;

            Categories.Clear();
            foreach (var c in categories) Categories.Add(c);
            SelectedCategory = defaultCategory;

            RaiseFieldFocus();
            IsOpen = true;
        }

        // ─── Keyboard ─────────────────────────────────────────────────────────

        private void SetActiveField(InlineEditorField field)
        {
            _activeField = field;
            RaiseFieldFocus();
        }

        private void RaiseFieldFocus()
        {
            OnPropertyChanged(nameof(IsNameActive));
            OnPropertyChanged(nameof(IsPriceActive));
        }

        private void AppendKey(string? key)
        {
            if (key == null) return;

            if (_activeField == InlineEditorField.Name)
            {
                Name += key;
            }
            else // Price — only allow digits and one decimal point
            {
                if (key == "." && Price.Contains('.')) return;
                if (key != "." && !char.IsDigit(key[0])) return;
                Price += key;
            }
        }

        private void Backspace()
        {
            if (_activeField == InlineEditorField.Name && Name.Length > 0)
                Name = Name[..^1];
            else if (_activeField == InlineEditorField.Price && Price.Length > 0)
                Price = Price[..^1];
        }

        private void ClearActiveField()
        {
            if (_activeField == InlineEditorField.Name) Name = string.Empty;
            else Price = string.Empty;
        }


        public void OpenForEditCategory(CategoryViewModel category)
        {
            Mode = InlineEditorMode.Category;
            _editingId = category.Id;        // ← add private int? _editingId field
            Name = category.Name;
            Price = string.Empty;
            _activeField = InlineEditorField.Name;
            RaiseFieldFocus();
            OnPropertyChanged(nameof(Title));
            IsOpen = true;
        }

        public void OpenForEditProduct(MenuItemViewModel product,
                                       IEnumerable<CategoryViewModel> categories,
                                       CategoryViewModel? currentCategory)
        {
            Mode = InlineEditorMode.Product;
            _editingId = product.Id;
            Name = product.Name;
            Price = product.Price.ToString("N2");
            _activeField = InlineEditorField.Name;

            Categories.Clear();
            foreach (var c in categories) Categories.Add(c);

            SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId)
                               ?? currentCategory;

            foreach (var c in Categories)
                c.RaiseIsSelected(SelectedCategory?.Id);

            RaiseFieldFocus();
            OnPropertyChanged(nameof(Title));
            IsOpen = true;
        }
        // ─── Save ─────────────────────────────────────────────────────────────

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            try
            {
                if (Mode == InlineEditorMode.Category)
                {
                    var saved = await _menuAdmin.SaveCategoryAsync(_editingId, Name);
                    SavedSuccessfully?.Invoke(saved);
                }
                else
                {
                    if (!decimal.TryParse(Price, out decimal price) || price < 0) return;
                    if (SelectedCategory == null) return;

                    var saved = await _menuAdmin.SaveProductAsync(
                        _editingId, Name, price, SelectedCategory.Id);
                    SavedSuccessfully?.Invoke(saved);
                }

                Close();
            }
            catch (Exception ex)
            {
                Name = $"[Error: {ex.Message}]";
            }
        }

        private void Close()
        {
            IsOpen = false;
            Cancelled?.Invoke();
        }
    }
}