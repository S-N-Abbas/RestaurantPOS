using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Cover
{
    public enum CoverField
    {
        Adults,
        Children
    }


    public class CoverSelectorViewModel : ViewModelBase
    {
        private OrderState _orderState;
        private readonly OrderService _orderService;
        private readonly IOrderContextService _orderContextService;
        private readonly SettingsService _settingsService;

        public event Action? RequestClose;

        // ─── New fields alongside existing AdultCovers / ChildCovers ─────────────────

        private string _coverALabel = string.Empty;
        public string CoverALabel
        {
            get => _coverALabel;
            set => SetProperty(ref _coverALabel, value);
        }

        private string _coverBLabel = string.Empty;
        public string CoverBLabel
        {
            get => _coverBLabel;
            set => SetProperty(ref _coverBLabel, value);
        }

        private string _coverAPrice = string.Empty;
        public string CoverAPrice
        {
            get => _coverAPrice;
            set => SetProperty(ref _coverAPrice, value);
        }

        private string _coverBPrice = string.Empty;
        public string CoverBPrice
        {
            get => _coverBPrice;
            set => SetProperty(ref _coverBPrice, value);
        }

        // Placeholders shown when fields are empty
        public string CoverAPlaceholder => _settingsService.Settings.AdultCoverLabel;
        public string CoverBPlaceholder => _settingsService.Settings.ChildCoverLabel;

        public CoverSelectorViewModel(OrderState orderState, OrderService orderService, IOrderContextService orderContextService, SettingsService settingsService)
        {
            _orderState = orderState;
            _orderService = orderService;
            _orderContextService = orderContextService;
            _settingsService = settingsService;

            _adultCovers = orderState.Order?.AdultCovers ?? 0;
            _childCovers = orderState.Order?.ChildCovers ?? 0;

            IncreaseAdultsCommand = new RelayCommand(() => AdultCovers++);
            DecreaseAdultsCommand = new RelayCommand(() =>
            {
                if (AdultCovers > 0) AdultCovers--;
            });

            IncreaseChildrenCommand = new RelayCommand(() => ChildCovers++);
            DecreaseChildrenCommand = new RelayCommand(() =>
            {
                if (ChildCovers > 0) ChildCovers--;
            });

            AppendDigitCommand = new RelayCommand<string>(AppendDigit);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearCommand = new RelayCommand(Clear);

            ConfirmCommand = new RelayCommand(async () => await ConfirmAsync());
        }

        // Covers
        private int _adultCovers;
        public int AdultCovers
        {
            get => _adultCovers;
            set => SetProperty(ref _adultCovers, Math.Max(0, value));
        }

        private int _childCovers;
        public int ChildCovers
        {
            get => _childCovers;
            set => SetProperty(ref _childCovers, Math.Max(0, value));
        }

        public void Reload(OrderState newState)
        {
            _orderState = newState;
            AdultCovers = _orderState.Order?.AdultCovers ?? 0;
            ChildCovers = _orderState.Order?.ChildCovers ?? 0;

            // Load per-order label overrides — or leave empty to show placeholder
            CoverALabel = _orderState.Order?.CoverALabel ?? string.Empty;
            CoverBLabel = _orderState.Order?.CoverBLabel ?? string.Empty;
            CoverAPrice = _orderState.Order?.CoverAPrice?.ToString("N2") ?? string.Empty;
            CoverBPrice = _orderState.Order?.CoverBPrice?.ToString("N2") ?? string.Empty;
        }

        private async Task ConfirmAsync()
        {
            if (_orderState.Order == null)
            {
                var order = await _orderService.CreateOrderAsync(
                    _orderState.ContextId,
                    _orderContextService.CurrentOrderType);
                _orderState.AttachOrder(order);
            }

            decimal? aPriceOverride = decimal.TryParse(CoverAPrice, out var ap) ? ap : null;
            decimal? bPriceOverride = decimal.TryParse(CoverBPrice, out var bp) ? bp : null;

            await _orderService.UpdateCoversWithLabelsAsync(
                _orderState.Order!.Id,
                AdultCovers,
                ChildCovers,
                string.IsNullOrWhiteSpace(CoverALabel) ? null : CoverALabel,
                aPriceOverride,
                string.IsNullOrWhiteSpace(CoverBLabel) ? null : CoverBLabel,
                bPriceOverride);

            // Update in-memory order object too
            _orderState.Order.AdultCovers = AdultCovers;
            _orderState.Order.ChildCovers = ChildCovers;
            _orderState.Order.CoverALabel = string.IsNullOrWhiteSpace(CoverALabel) ? null : CoverALabel;
            _orderState.Order.CoverBLabel = string.IsNullOrWhiteSpace(CoverBLabel) ? null : CoverBLabel;
            _orderState.Order.CoverAPrice = aPriceOverride;
            _orderState.Order.CoverBPrice = bPriceOverride;

            RequestClose?.Invoke();
        }


        // Active field tracking (CRITICAL for keypad)
        private CoverField _activeField = CoverField.Adults;

        public void SetActiveField(CoverField field)
        {
            _activeField = field;
        }

        // ✅ Keypad Logic
        private void AppendDigit(string digit)
        {
            if (!int.TryParse(digit, out int d))
                return;

            switch (_activeField)
            {
                case CoverField.Adults:
                    AdultCovers = Append(AdultCovers, d);
                    break;

                case CoverField.Children:
                    ChildCovers = Append(ChildCovers, d);
                    break;
            }
        }

        private static int Append(int current, int digit)
        {
            if (current == 0)
                return digit;

            return (current * 10) + digit;
        }

        private void Backspace()
        {
            switch (_activeField)
            {
                case CoverField.Adults:
                    AdultCovers /= 10;
                    break;

                case CoverField.Children:
                    ChildCovers /= 10;
                    break;
            }
        }

        private void Clear()
        {
            switch (_activeField)
            {
                case CoverField.Adults:
                    AdultCovers = 0;
                    break;

                case CoverField.Children:
                    ChildCovers = 0;
                    break;
            }
        }

        // Commands
        public ICommand IncreaseAdultsCommand { get; }
        public ICommand DecreaseAdultsCommand { get; }

        public ICommand IncreaseChildrenCommand { get; }
        public ICommand DecreaseChildrenCommand { get; }

        public ICommand AppendDigitCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearCommand { get; }

        public ICommand ConfirmCommand { get; }
    }

}
