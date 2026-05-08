using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Orders.History;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Orders.History
{
    public class OrderHistoryViewModel : ViewModelBase
    {
        private readonly OrderService _orderService;
        private readonly SettingsService _settingsService;
        private readonly ReceiptBuilder _receiptBuilder;
        private readonly UserSessionService _userSessionService;

        // ─── Filters ──────────────────────────────────────────────────────────

        private DateTime _from = DateTime.Today;
        public DateTime From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        private DateTime _to = DateTime.Today.AddDays(1).AddSeconds(-1);
        public DateTime To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        // Order type filter
        public IReadOnlyList<string> OrderTypeFilters { get; } =
            new[] { "All", "Dine-In", "TakeAway", "Delivery" };

        private string _selectedTypeFilter = "All";
        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set => SetProperty(ref _selectedTypeFilter, value);
        }

        // Status filter
        public IReadOnlyList<string> StatusFilters { get; } =
            new[] { "All", "Paid", "Cancelled" };

        private string _selectedStatusFilter = "All";
        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set => SetProperty(ref _selectedStatusFilter, value);
        }

        // ─── List ─────────────────────────────────────────────────────────────

        public ObservableCollection<OrderHistoryItemViewModel> Orders { get; } = new();

        private OrderHistoryItemViewModel? _selectedOrder;
        public OrderHistoryItemViewModel? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        // ─── State ────────────────────────────────────────────────────────────

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        public bool HasSelection => SelectedOrder != null;

        // Summary totals shown below the list
        private int _totalOrders;
        private decimal _totalIncome;
        private decimal _totalCash;
        private decimal _totalCard;

        public string SummaryOrders => _totalOrders.ToString();
        public string SummaryIncome => $"{_settingsService.Settings.CurrencySymbol}{_totalIncome:N2}";
        public string SummaryCash => $"{_settingsService.Settings.CurrencySymbol}{_totalCash:N2}";
        public string SummaryCard => $"{_settingsService.Settings.CurrencySymbol}{_totalCard:N2}";

        // ─── Quick date shortcuts ─────────────────────────────────────────────

        public ICommand SetTodayCommand { get; }
        public ICommand SetYesterdayCommand { get; }
        public ICommand SetThisWeekCommand { get; }
        public ICommand SetThisMonthCommand { get; }

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand SearchCommand { get; }
        public ICommand SelectOrderCommand { get; }
        public ICommand ReprintCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public OrderHistoryViewModel(
            OrderService orderService,
            SettingsService settingsService,
            UserSessionService userSessionService)
        {
            _orderService = orderService;
            _settingsService = settingsService;
            _userSessionService = userSessionService;
            _receiptBuilder = new ReceiptBuilder(settingsService, userSessionService);

            SetTodayCommand = new RelayCommand(() => SetRange(
                DateTime.Today,
                DateTime.Today.AddDays(1).AddSeconds(-1)));

            SetYesterdayCommand = new RelayCommand(() => SetRange(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddSeconds(-1)));

            SetThisWeekCommand = new RelayCommand(() =>
            {
                var start = DateTime.Today.AddDays(
                    -(int)DateTime.Today.DayOfWeek + 1);
                SetRange(start, DateTime.Now);
            });

            SetThisMonthCommand = new RelayCommand(() =>
            {
                var start = new DateTime(
                    DateTime.Today.Year, DateTime.Today.Month, 1);
                SetRange(start, DateTime.Now);
            });

            SearchCommand = new RelayCommand(async () => await SearchAsync());
            SelectOrderCommand = new RelayCommand<OrderHistoryItemViewModel>(OnSelectOrder);
            ReprintCommand = new RelayCommand(Reprint, () => HasSelection);
            ClearSelectionCommand = new RelayCommand(() => SelectedOrder = null);

            // Load today on startup
            _ = SearchAsync();
        }

        // ─── Logic ────────────────────────────────────────────────────────────

        private void SetRange(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        private async Task SearchAsync()
        {
            IsLoading = true;
            IsEmpty = false;
            SelectedOrder = null;

            try
            {
                // Map filter strings to enum values
                OrderType? typeFilter = _selectedTypeFilter switch
                {
                    "Dine-In" => OrderType.DineIn,
                    "TakeAway" => OrderType.TakeAway,
                    "Delivery" => OrderType.Delivery,
                    _ => null
                };

                OrderStatus? statusFilter = _selectedStatusFilter switch
                {
                    "Paid" => OrderStatus.Paid,
                    "Cancelled" => OrderStatus.Cancelled,
                    _ => null
                };

                var orders = await _orderService.GetOrderHistoryAsync(
                    From, To, typeFilter, statusFilter);

                Orders.Clear();

                foreach (var order in orders)
                    Orders.Add(new OrderHistoryItemViewModel(order, _settingsService));

                // Calculate summary totals
                _totalOrders = Orders.Count;
                _totalCash = Orders.Sum(o => o.CashPaid);
                _totalCard = Orders.Sum(o => o.CardPaid);
                _totalIncome = Orders.Sum(o => o.GrandTotal);

                RaiseSummary();

                IsEmpty = !Orders.Any();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSelectOrder(OrderHistoryItemViewModel? order)
        {
            SelectedOrder = order;
            OnPropertyChanged(nameof(HasSelection));
            ((RelayCommand)ReprintCommand).NotifyCanExecuteChanged();
        }

        private void Reprint()
        {
            if (SelectedOrder == null) return;

            // Rebuild a minimal Order object for the receipt builder
            // We can't pass the VM — receipt builder needs the entity
            _ = ReprintAsync();
        }

        private async Task ReprintAsync()
        {
            if (SelectedOrder == null) return;

            try
            {
                // Reload from DB to get full navigation properties
                var order = await _orderService.GetByIdAsync(SelectedOrder.Id);

                var doc = _receiptBuilder.Build(order);

                var dialog = new PrintDialog();
                if (dialog.ShowDialog() == true)
                {
                    dialog.PrintDocument(
                        ((IDocumentPaginatorSource)doc).DocumentPaginator,
                        $"Receipt #{order.Id}");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Print failed: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void RaiseSummary()
        {
            OnPropertyChanged(nameof(SummaryOrders));
            OnPropertyChanged(nameof(SummaryIncome));
            OnPropertyChanged(nameof(SummaryCash));
            OnPropertyChanged(nameof(SummaryCard));
        }
    }
}