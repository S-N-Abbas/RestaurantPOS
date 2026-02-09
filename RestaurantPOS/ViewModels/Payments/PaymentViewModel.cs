using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly OrderState _orderState;
        private readonly OrderService _orderService;
        private readonly INavigationService _navigationService;

        private readonly IPaymentService _paymentService;
        private readonly Order _order;

        public decimal Total => _orderState.Order?.TotalAmount ?? 0m;

        private decimal _paid;
        public decimal Paid
        {
            get => _paid;
            private set
            {
                if (SetProperty(ref _paid, value))
                {
                    OnPropertyChanged(nameof(Due));
                    OnPropertyChanged(nameof(CanPay));
                }
            }
        }
        public decimal Due => Math.Max(0, Total - Paid);

        private string _enteredAmount = "";
        public string EnteredAmount
        {
            get => _enteredAmount;
            set => SetProperty(ref _enteredAmount, value);
        }

        private PaymentMethod _method = PaymentMethod.Cash;
        public PaymentMethod Method
        {
            get => _method;
            set => SetProperty(ref _method, value);
        }

        public bool CanPay => _orderState.Order != null && Due <= 0;

        public string TableLabel => $"Table {_orderState.TableNumber}";

        // Commands
        public ICommand AppendDigitCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearAmountCommand { get; }

        public ICommand SelectCashCommand { get; }
        public ICommand SelectCardCommand { get; }
        public ICommand SplitBillCommand { get; }

        public ICommand PayCommand { get; }
        public ICommand CancelCommand { get; }

        public PaymentViewModel(
        OrderState orderState,
        OrderService orderService,
        INavigationService navigationService)
        {
            _orderState = orderState;
            _orderService = orderService;
            _navigationService = navigationService;

            Paid = orderState.Order?.PaidAmount ?? 0m;

            AppendDigitCommand = new RelayCommand<string>(AppendDigit);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearAmountCommand = new RelayCommand(() => EnteredAmount = "");

            SelectCashCommand = new RelayCommand(() => Method = PaymentMethod.Cash);
            SelectCardCommand = new RelayCommand(SelectCard);
            SplitBillCommand = new RelayCommand(() => Method = PaymentMethod.Split);

            PayCommand = new AsyncRelayCommand(PayAsync);
            CancelCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<TablesViewModel>());
        }

        // -----------------------------
        // INPUT
        // -----------------------------
        private void AppendDigit(string digit)
        {
            if (EnteredAmount.Length >= 6)
                return;

            EnteredAmount += digit;
        }

        private void Backspace()
        {
            if (EnteredAmount.Length > 0)
                EnteredAmount = EnteredAmount[..^1];
        }

        private decimal ParseAmount()
        {
            return decimal.TryParse(EnteredAmount, out var value)
                ? value
                : 0m;
        }

        // -----------------------------
        // PAYMENT FLOW
        // -----------------------------
        private void SelectCard()
        {
            Method = PaymentMethod.Card;
            EnteredAmount = Due.ToString("0.00");
        }

        private async Task PayAsync()
        {
            if (_orderState.Order == null)
                return;

            var amount = ParseAmount();
            if (amount <= 0)
                return;

            // Card must match remaining due
            if (Method == PaymentMethod.Card)
                amount = Due;

            Paid += amount;
            EnteredAmount = "";

            await _orderService.RecordPaymentAsync(
                _orderState.Order.Id,
                amount,
                Method.ToString());

            if (Due <= 0)
            {
                await _orderService.CloseOrderAsync(_orderState.Order.Id);
                _navigationService.NavigateTo<TablesViewModel>();
            }

            var updatedOrder = await _orderService.GetByIdAsync(_orderState.Order.Id);
            _orderState.UpdateFrom(updatedOrder);
        }
    }
}
