using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Home;
using RestaurantPOS.ViewModels.Orders;
using RestaurantPOS.ViewModels.Tables;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Payments
{
    public enum PaymentMethod
    {
        Cash,
        Card
    }

    public class PaymentViewModel : ViewModelBase
    {
        private readonly OrderService _orderService;
        private readonly SettingsService _settingsService;
        private readonly INavigationService _navigationService;
        private readonly UserSessionService _userSessionService;

        private readonly ReceiptBuilder _receiptBuilder;
        public ICommand PrintReceiptCommand => new RelayCommand(PrintReceipt);

        public OrderState _orderState { get; }
        public OrderStore _orderStore { get; }

        public decimal Change => IsCashSelected && EnteredAmount > Due
    ? EnteredAmount - Due
    : 0m;

        public bool HasChange => Change > 0;

        public bool IsCashSelected => SelectedMethod == PaymentMethod.Cash;


        public string TableLabel => _orderState.ContextId > 0 ? $"Table {_orderState.ContextId}" : $"{_orderState.Order?.OrderType.ToString()}";

        // Payment amounts

        public int AdultCount => _orderState.Order?.AdultCovers ?? 0;
        public int ChildCount => _orderState.Order?.ChildCovers ?? 0;

        public decimal AdultCoverTotal => _orderState.Order == null
            ? 0m
            : _settingsService.CalculateAdultCoverCharge(_orderState.Order);
        public decimal ChildCoverTotal => _orderState.Order == null
            ? 0m
            : _settingsService.CalculateChildCoverCharge(_orderState.Order);
        public decimal CoverTotal => _orderState.Order == null
            ? 0m
            : _settingsService.CalculateCoverCharge(_orderState.Order);

        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        public decimal Total =>
            (_orderState.Order?.ItemsTotal ?? 0m) + CoverTotal;

        public decimal AlreadyPaid => _orderState.Order?.PaidAmount ?? 0;
        public decimal PreviewPaid => AlreadyPaid;

        public decimal Due => Math.Max(Total - AlreadyPaid, 0);
        public decimal PreviewDue => Math.Max(Total - AlreadyPaid, 0);

        // Commands
        public ICommand CancelCommand { get; }
        public ICommand PayCommand { get; }
        public ICommand AppendDigitCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearAmountCommand { get; }
        public ICommand SelectCashCommand { get; }
        public ICommand SelectCardCommand { get; }


        private decimal _enteredAmount;
        public decimal EnteredAmount
        {
            get => _enteredAmount;
            set
            {
                if (SetProperty(ref _enteredAmount, value))
                {
                    OnPropertyChanged(nameof(PreviewPaid));
                    OnPropertyChanged(nameof(PreviewDue));
                    OnPropertyChanged(nameof(AlreadyPaid));
                    OnPropertyChanged(nameof(Due));
                    OnPropertyChanged(nameof(CanPay));
                    OnPropertyChanged(nameof(Change));
                    OnPropertyChanged(nameof(HasChange));
                    (PayCommand as RelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        private PaymentMethod? _selectedMethod;
        public PaymentMethod? SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                if (SetProperty(ref _selectedMethod, value))
                {
                    OnPropertyChanged(nameof(CanPay));
                    OnPropertyChanged(nameof(IsCashSelected));
                    OnPropertyChanged(nameof(Change));
                    OnPropertyChanged(nameof(HasChange));
                }
                (PayCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private void PrintReceipt()
        {
            if (_orderState.Order == null)
                return;

            var doc = _receiptBuilder.Build(_orderState.Order);

            var printerName = _settingsService.Settings.DefaultPrinter;

            // ✅ If a default printer is configured, print silently
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                try
                {
                    var dialog = new PrintDialog();
                    dialog.PrintQueue = new System.Printing.PrintQueue(
                        new System.Printing.PrintServer(),
                        printerName);

                    dialog.PrintDocument(
                        ((IDocumentPaginatorSource)doc).DocumentPaginator,
                        "Receipt");

                    return;
                }
                catch (Exception ex)
                {
                    // Printer not found or unavailable — fall through to dialog
                    System.Diagnostics.Debug.WriteLine(
                        $"Default printer '{printerName}' unavailable: {ex.Message}");
                }
            }

            // ✅ Fallback — show dialog if no default printer or it failed
            var fallbackDialog = new PrintDialog();
            if (fallbackDialog.ShowDialog() == true)
            {
                fallbackDialog.PrintDocument(
                    ((IDocumentPaginatorSource)doc).DocumentPaginator,
                    "Receipt");
            }
        }

        public bool CanPay =>
            SelectedMethod != null &&
            EnteredAmount > 0 &&
            (IsCashSelected
        ? EnteredAmount > 0          // ✅ cash: allow exact or over
        : EnteredAmount <= Due);        // ✅ card: must not exceed remaining

        public PaymentViewModel(OrderState orderState, OrderService orderService, OrderStore orderStore, SettingsService settingsService, INavigationService navigationService, UserSessionService userSessionService)
        {
            _orderState = orderState;
            _orderService = orderService;
            _orderStore = orderStore;
            _settingsService = settingsService;
            _navigationService = navigationService;
            _userSessionService = userSessionService;

            _settingsService.SettingsChanged += () => OnPropertyChanged(nameof(CurrencySymbol));

            _receiptBuilder = new ReceiptBuilder(_settingsService, _userSessionService);



            SelectedMethod = PaymentMethod.Cash; // default to cash

            // Commands
            PayCommand = new RelayCommand(async () => await PayAsync(), () => CanPay);
            CancelCommand = new RelayCommand(() => _navigationService.NavigateTo<TablesViewModel>());

            AppendDigitCommand = new RelayCommand<string>(AppendDigit);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearAmountCommand = new RelayCommand(() => EnteredAmount = 0);

            SelectCashCommand = new RelayCommand(() => SelectedMethod = PaymentMethod.Cash);
            SelectCardCommand = new RelayCommand(() => SelectedMethod = PaymentMethod.Card);
        }

        private async Task PayAsync()
        {
            if (_orderState.Order == null || SelectedMethod == null)
                return;

            await _orderService.RecordPaymentAsync(
                _orderState.Order.Id,
                EnteredAmount,
                SelectedMethod.ToString());

            // Reload to get updated PaidAmount
            var updatedOrder = await _orderService.GetByIdAsync(_orderState.Order.Id);
            _orderState.UpdateFrom(updatedOrder);

            EnteredAmount = 0;
            OnPropertyChanged(nameof(Change));
            OnPropertyChanged(nameof(HasChange));

            // ✅ PaidAmount now includes tendered cash — will be >= Total when paid in full
            if (_orderState.Order.PaidAmount >= Total)
            {
                await _orderService.CloseOrderAsync(_orderState.Order.Id);
                var closedOrder = await _orderService.GetByIdAsync(_orderState.Order.Id);
                _orderState.UpdateFrom(closedOrder);

                _orderStore.CloseOrder(_orderState.Order.ContextId);

                PrintReceipt();

                if(_orderState.Order.OrderType == OrderType.TakeAway || _orderState.Order.OrderType == OrderType.Delivery)
                    _navigationService.NavigateBackTo<HomeViewModel>();
                else
                    _navigationService.NavigateBackTo<TablesViewModel>();
            }
        }


        private void AppendDigit(string digit)
        {
            string current = ((int)(EnteredAmount * 100)).ToString();
            current += digit;
            if (long.TryParse(current, out var val))
            {
                EnteredAmount = val / 100m;
            }
        }

        private void Backspace()
        {
            string current = ((int)(EnteredAmount * 100)).ToString();
            if (current.Length > 1)
            {
                current = current.Substring(0, current.Length - 1);
                if (long.TryParse(current, out var val))
                    EnteredAmount = val / 100m;
            }
            else
            {
                EnteredAmount = 0;
            }
        }
    }
}
