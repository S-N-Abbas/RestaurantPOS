// ViewModels/Orders/OpenItemEditorViewModel.cs

using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Orders
{
    public enum OpenItemField
    {
        Name,
        Price
    }

    public class OpenItemEditorViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        // ─── Visibility ───────────────────────────────────────────────────────

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        // ─── Fields ───────────────────────────────────────────────────────────

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _priceDisplay = string.Empty;
        public string PriceDisplay
        {
            get => _priceDisplay;
            set => SetProperty(ref _priceDisplay, value);
        }

        public decimal Price =>
            decimal.TryParse(_priceDisplay, out var p) ? p : 0m;

        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        // ─── Active field ─────────────────────────────────────────────────────

        private OpenItemField _activeField = OpenItemField.Name;

        public bool IsNameActive => _activeField == OpenItemField.Name;
        public bool IsPriceActive => _activeField == OpenItemField.Price;

        // ─── Events ───────────────────────────────────────────────────────────

        /// <summary>Fired when user confirms. Carries name and price.</summary>
        public event Action<string, decimal>? Confirmed;

        public event Action? Cancelled;

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand FocusNameCommand { get; }
        public ICommand FocusPriceCommand { get; }
        public ICommand KeyCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public ICommand OpenItemNamePriceChanged { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public OpenItemEditorViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;

            FocusNameCommand = new RelayCommand(() => SetField(OpenItemField.Name));
            FocusPriceCommand = new RelayCommand(() => SetField(OpenItemField.Price));

            KeyCommand = new RelayCommand<string>(AppendKey);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearCommand = new RelayCommand(ClearActive);

            ConfirmCommand = new RelayCommand(
                OnConfirm,
                () => !string.IsNullOrWhiteSpace(Name) && Price > 0);

            OpenItemNamePriceChanged = new RelayCommand(() =>
            {
                ((RelayCommand)ConfirmCommand).NotifyCanExecuteChanged();
            });

            CancelCommand = new RelayCommand(Close);

            _settingsService.SettingsChanged += ()
                => OnPropertyChanged(nameof(CurrencySymbol));
        }

        // ─── Open ─────────────────────────────────────────────────────────────

        public void Open()
        {
            Name = string.Empty;
            PriceDisplay = string.Empty;
            _activeField = OpenItemField.Name;
            RaiseFieldFocus();
            ((RelayCommand)ConfirmCommand).NotifyCanExecuteChanged();
            IsOpen = true;
        }

        // ─── Keyboard ─────────────────────────────────────────────────────────

        private void SetField(OpenItemField field)
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

            if (_activeField == OpenItemField.Name)
            {
                Name += key;
            }
            else
            {
                // Price — digits and one decimal point only
                if (key == "." && PriceDisplay.Contains('.')) return;
                if (key != "." && !char.IsDigit(key[0])) return;
                PriceDisplay += key;
            }
        }

        private void Backspace()
        {
            if (_activeField == OpenItemField.Name && Name.Length > 0)
                Name = Name[..^1];
            else if (_activeField == OpenItemField.Price && PriceDisplay.Length > 0)
                PriceDisplay = PriceDisplay[..^1];
        }

        private void ClearActive()
        {
            if (_activeField == OpenItemField.Name) Name = string.Empty;
            else PriceDisplay = string.Empty;
        }

        // ─── Confirm / Cancel ─────────────────────────────────────────────────

        private void OnConfirm()
        {
            if (string.IsNullOrWhiteSpace(Name) || Price <= 0) return;
            Confirmed?.Invoke(Name.Trim(), Price);
            Close();
        }

        private void Close()
        {
            IsOpen = false;
            Cancelled?.Invoke();
        }
    }
}