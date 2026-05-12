using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;

namespace RestaurantPOS.ViewModels.Orders.History
{
    /// <summary>
    /// Read-only VM for a single order card in the history list.
    /// </summary>
    public class OrderHistoryItemViewModel : ViewModelBase
    {
        private readonly Order _order;
        private readonly SettingsService _settingsService;

        // ─── Identity ─────────────────────────────────────────────────────────
        // In OrderHistoryItemViewModel.cs — add this property

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);  // needs ViewModelBase
        }


        public int Id => _order.Id;
        public string OrderLabel => $"#{_order.Id:D4}";

        // ─── Type & Context ───────────────────────────────────────────────────

        public OrderType OrderType => _order.OrderType;

        public string TypeLabel => _order.OrderType switch
        {
            OrderType.DineIn => "Dine-In",
            OrderType.TakeAway => "TakeAway",
            OrderType.Delivery => "Delivery",
            _ => "Order"
        };

        public string ContextLabel => _order.OrderType switch
        {
            OrderType.DineIn => $"Table {_order.ContextId}",
            OrderType.TakeAway => $"TA #{Math.Abs(_order.ContextId)}",
            OrderType.Delivery => $"DEL #{Math.Abs(_order.ContextId)}",
            _ => $"#{_order.ContextId}"
        };

        public string TypeIcon => _order.OrderType switch
        {
            OrderType.TakeAway => "BagPersonal",
            OrderType.Delivery => "Moped",
            _ => "TableChair"
        };

        // ─── Status ───────────────────────────────────────────────────────────

        public OrderStatus Status => _order.Status;

        public string StatusLabel => _order.Status switch
        {
            OrderStatus.Paid => "Paid",
            OrderStatus.Cancelled => "Cancelled",
            _ => "Closed"
        };

        public string StatusColour => _order.Status switch
        {
            OrderStatus.Paid => "#D1FAE5",
            OrderStatus.Cancelled => "#FEE2E2",
            _ => "#F1F5F9"
        };

        public string StatusTextColour => _order.Status switch
        {
            OrderStatus.Paid => "#065F46",
            OrderStatus.Cancelled => "#991B1B",
            _ => "#475569"
        };

        // ─── Time ─────────────────────────────────────────────────────────────

        public string DateDisplay => _order.ClosedAt.HasValue
            ? _order.ClosedAt.Value.ToString("dd MMM yyyy")
            : _order.CreatedAt.ToString("dd MMM yyyy");

        public string TimeDisplay => _order.ClosedAt.HasValue
            ? _order.ClosedAt.Value.ToString("HH:mm")
            : _order.CreatedAt.ToString("HH:mm");

        public string FullDateTimeDisplay => _order.ClosedAt.HasValue
            ? _order.ClosedAt.Value.ToString("dd MMM yyyy  HH:mm")
            : _order.CreatedAt.ToString("dd MMM yyyy  HH:mm");

        // ─── Staff ────────────────────────────────────────────────────────────

        public string CreatedBy => string.IsNullOrWhiteSpace(_order.CreatedBy)
            ? "Unknown" : _order.CreatedBy;

        public string ClosedBy => string.IsNullOrWhiteSpace(_order.ClosedBy)
            ? "Unknown" : _order.ClosedBy;

        // ─── Financials ───────────────────────────────────────────────────────

        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        public decimal ItemsTotal => _order.ItemsTotal;
        public decimal CoverTotal => _settingsService.CalculateCoverCharge(_order);
        public decimal GrandTotal => ItemsTotal + CoverTotal;

        public decimal CashPaid
        {
            get
            {
                decimal rawCard = _order.Payments
                    .Where(p => p.Method == "Card").Sum(p => p.Amount);
                decimal rawDeposit = _order.Payments
                    .Where(p => p.Method == "Deposit").Sum(p => p.Amount);
                decimal rawCash = _order.Payments
                    .Where(p => p.Method == "Cash").Sum(p => p.Amount);

                decimal covered = rawCard + rawDeposit;
                decimal netCash = Math.Max(GrandTotal - covered, 0m);
                return Math.Min(netCash, rawCash);
            }
        }
        public decimal CardPaid => _order.Payments
            .Where(p => p.Method == "Card").Sum(p => p.Amount);
        public decimal DepositPaid => _order.Payments
            .Where(p => p.Method == "Deposit").Sum(p => p.Amount);

        public string GrandTotalDisplay => $"{CurrencySymbol}{GrandTotal:N2}";

        // ─── Items ────────────────────────────────────────────────────────────

        public IReadOnlyList<OrderItem> Items =>
            _order.Items.OrderBy(i => i.ProductName).ToList();

        // ─── Covers ───────────────────────────────────────────────────────────

        public bool HasCovers =>
            _order.AdultCovers > 0 || _order.ChildCovers > 0;

        public int AdultCovers => _order.AdultCovers;
        public int ChildCovers => _order.ChildCovers;

        public string CoverALabel =>
            _order.CoverALabel ?? _settingsService.Settings.AdultCoverLabel ?? "Adults";

        public string CoverBLabel =>
            _order.CoverBLabel ?? _settingsService.Settings.ChildCoverLabel ?? "Children";

        public decimal CoverAUnitPrice =>
            _order.CoverAPrice ?? _settingsService.Settings.AdultCoverPrice;

        public decimal CoverBUnitPrice =>
            _order.CoverBPrice ?? _settingsService.Settings.ChildCoverPrice;

        public decimal CoverATotal => AdultCovers * CoverAUnitPrice;
        public decimal CoverBTotal => ChildCovers * CoverBUnitPrice;

        // ─── Constructor ──────────────────────────────────────────────────────

        public OrderHistoryItemViewModel(Order order, SettingsService settingsService)
        {
            _order = order;
            _settingsService = settingsService;
        }
    }
}