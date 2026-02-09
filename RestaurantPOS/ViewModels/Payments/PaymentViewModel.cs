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
        public decimal AlreadyPaid => OrderState.Order?.PaidAmount ?? 0;
        public decimal PreviewPaid => AlreadyPaid + EnteredAmount;

        public decimal Due => Math.Max(Total - AlreadyPaid, 0);
        public decimal PreviewDue => Math.Max(Total - PreviewPaid, 0);

        // Commands
        public ICommand CancelCommand { get; }
        public ICommand PayCommand { get; }
        public ICommand AppendDigitCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearAmountCommand { get; }
        public ICommand SelectCashCommand { get; }
        public ICommand SelectCardCommand { get; }
        public ICommand SplitBillCommand { get; }


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
                    OnPropertyChanged(nameof(CanPay));
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
                    OnPropertyChanged(nameof(CanPay));
                (PayCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }


        public bool CanPay =>
            SelectedMethod != null &&
            EnteredAmount > 0 &&
            EnteredAmount <= Due;

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

            SelectCashCommand = new RelayCommand(() => SelectedMethod = PaymentMethod.Cash);
            SelectCardCommand = new RelayCommand(() => SelectedMethod = PaymentMethod.Card);
            SplitBillCommand = new RelayCommand(() => SelectedMethod = PaymentMethod.Split);
        }

        private async Task PayAsync()
        {
            if (OrderState.Order == null || SelectedMethod == null)
                return;

            await _orderService.RecordPaymentAsync(
                OrderState.Order.Id,
                EnteredAmount,
                SelectedMethod.ToString() // convert enum to string for DB

            );

            EnteredAmount = 0;

            if (OrderState.Order.PaidAmount >= OrderState.Order.TotalAmount)
            {
                await _orderService.CloseOrderAsync(OrderState.Order.Id);

                _navigationService.NavigateTo<TablesViewModel>();
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

        private void SplitBill()
        {
            SelectedMethod = PaymentMethod.Split;
            EnteredAmount = Math.Round(PreviewDue / 2, 2); // default half payment
        }


        private async Task ExecutePayAsync()
        {
            if (OrderState.Order == null) return;

            try
            {
                await _orderService.RecordPaymentAsync(OrderState.Order.Id, EnteredAmount, SelectedMethod.ToString()!);

                // reset entered amount
                EnteredAmount = 0;

                // If fully paid, navigate back
                if (PreviewDue <= 0)
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
