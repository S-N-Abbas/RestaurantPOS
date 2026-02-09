using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Tables;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Payments
{
    public enum PaymentMethod
    {
        Cash,
        Card,
        Split
    }

    public class PaymentViewModel : ViewModelBase
    {
        private readonly OrderService _orderService;
        private readonly INavigationService _navigationService;

        public OrderState OrderState { get; }

        // UK currency labels
        public string TableLabel => $"Table {OrderState.TableNumber}";

        // Payment amounts
        public decimal Total => OrderState.Order?.TotalAmount ?? 0;
        public decimal Paid => OrderState.Order?.PaidAmount ?? 0;
        public decimal Due => Math.Max(Total - Paid, 0);

        private decimal _enteredAmount;
        public decimal EnteredAmount
        {
            get => _enteredAmount;
            set
            {
                if (SetProperty(ref _enteredAmount, value))
                    OnPropertyChanged(nameof(CanPay));
            }
        }

        private string? _selectedMethod;
        public string? SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                if (SetProperty(ref _selectedMethod, value))
                    OnPropertyChanged(nameof(CanPay));
            }
        }

        public bool CanPay => EnteredAmount > 0 && !string.IsNullOrEmpty(SelectedMethod);

        // Commands
        public ICommand CancelCommand { get; }
        public ICommand PayCommand { get; }
        public ICommand AppendDigitCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearAmountCommand { get; }
        public ICommand SelectCashCommand { get; }
        public ICommand SelectCardCommand { get; }
        public ICommand SplitBillCommand { get; }

        public PaymentViewModel(OrderState orderState, OrderService orderService, INavigationService navigationService)
        {
            OrderState = orderState;
            _orderService = orderService;
            _navigationService = navigationService;

            // Commands
            PayCommand = new RelayCommand(async () => await PayAsync(), () => CanPay);
            CancelCommand = new RelayCommand(() => _navigationService.NavigateTo<TablesViewModel>());

            AppendDigitCommand = new RelayCommand<string>(AppendDigit);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearAmountCommand = new RelayCommand(() => EnteredAmount = 0);

            SelectCashCommand = new RelayCommand(() => SelectedMethod = "Cash");
            SelectCardCommand = new RelayCommand(() => SelectedMethod = "Card");
            SplitBillCommand = new RelayCommand(SplitBill);
        }

        private async Task PayAsync()
        {
            if (OrderState.Order == null)
                return;

            var method = "Cash"; // or use selected method
            await _orderService.RecordPaymentAsync(OrderState.Order.Id, EnteredAmount, method);

            // Refresh totals after payment
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Paid));
            OnPropertyChanged(nameof(Due));

            EnteredAmount = 0;

            // Optionally navigate back if fully paid
            if (OrderState.Order.PaidAmount >= OrderState.Order.TotalAmount)
                _navigationService.NavigateTo<TablesViewModel>();
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

        private void SplitBill()
        {
            SelectedMethod = "Split";
            EnteredAmount = Math.Round(Due / 2, 2); // default half payment
        }

        private async Task ExecutePayAsync()
        {
            if (OrderState.Order == null) return;

            try
            {
                await _orderService.RecordPaymentAsync(OrderState.Order.Id, EnteredAmount, SelectedMethod!);

                // reset entered amount
                EnteredAmount = 0;

                // refresh totals
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(Paid));
                OnPropertyChanged(nameof(Due));

                // If fully paid, navigate back
                if (Due <= 0)
                {
                    await _orderService.CloseOrderAsync(OrderState.Order.Id);
                    _navigationService.NavigateTo<Orders.OrderViewModel>();
                }
            }
            catch (Exception ex)
            {
                // TODO: show user-friendly error message
                Console.WriteLine(ex.Message);
            }
        }
    }
}
