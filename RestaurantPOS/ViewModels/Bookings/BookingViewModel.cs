using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Bookings
{
    /// <summary>
    /// Represents a single booking card in the BookingsView list.
    /// Owns its own action commands — confirm, seat, cancel, no-show.
    /// The parent BookingsViewModel subscribes to this VM's events
    /// to handle navigation and list refresh after an action completes.
    /// </summary>
    public class BookingSlotViewModel : ViewModelBase
    {
        private readonly IBookingService _bookingService;
        private readonly SettingsService _settingsService;

        // ─── Raw data ─────────────────────────────────────────────────────────

        private Booking _booking;

        // ─── Identity ─────────────────────────────────────────────────────────

        public int Id => _booking.Id;
        public string CustomerName => _booking.CustomerName;
        public string CustomerPhone => _booking.CustomerPhone;
        public string CustomerEmail => _booking.CustomerEmail;
        public int PartySize => _booking.PartySize;
        public string Notes => _booking.Notes;

        // ─── Time Display ─────────────────────────────────────────────────────

        /// <summary>e.g. "7:30 PM"</summary>
        public string BookingTime => _booking.BookingDate.ToString("h:mm tt");

        /// <summary>e.g. "Wed 15 Apr 2026"</summary>
        public string BookingDateDisplay => _booking.BookingDate.ToString("ddd d MMM yyyy");

        /// <summary>
        /// Human-readable countdown shown on the card.
        /// e.g. "In 2 hrs 15 min" / "In 45 min" / "Now" / "45 min ago"
        /// </summary>
        public string TimeUntilDisplay
        {
            get
            {
                var span = _booking.TimeUntil;

                if (span.TotalMinutes < -30)
                    return $"{(int)Math.Abs(span.TotalMinutes)} min ago";

                if (span.TotalMinutes < 0)
                    return "Now";

                if (span.TotalHours >= 1)
                    return $"In {(int)span.TotalHours}h {span.Minutes}min";

                return $"In {(int)span.TotalMinutes} min";
            }
        }

        // ─── Table ────────────────────────────────────────────────────────────

        public bool HasTable => _booking.TableId.HasValue;
        public string TableLabel => _booking.Table != null
            ? $"Table {_booking.Table.Number}"
            : "No table assigned";

        // ─── Status ───────────────────────────────────────────────────────────

        public BookingStatus Status => _booking.Status;

        public string StatusLabel => _booking.Status switch
        {
            BookingStatus.Pending => "Pending",
            BookingStatus.Confirmed => "Confirmed",
            BookingStatus.Seated => "Seated",
            BookingStatus.Completed => "Completed",
            BookingStatus.Cancelled => "Cancelled",
            BookingStatus.NoShow => "No Show",
            _ => "Unknown"
        };

        /// <summary>Colour hex for the status badge background.</summary>
        public string StatusColour => _booking.Status switch
        {
            BookingStatus.Pending => "#FEF3C7",   // amber
            BookingStatus.Confirmed => "#DBEAFE",   // blue
            BookingStatus.Seated => "#D1FAE5",   // green
            BookingStatus.Completed => "#F1F5F9",   // grey
            BookingStatus.Cancelled => "#FEE2E2",   // red
            BookingStatus.NoShow => "#FEE2E2",   // red
            _ => "#F1F5F9"
        };

        /// <summary>Colour hex for the status badge foreground text.</summary>
        public string StatusTextColour => _booking.Status switch
        {
            BookingStatus.Pending => "#92400E",
            BookingStatus.Confirmed => "#1E40AF",
            BookingStatus.Seated => "#065F46",
            BookingStatus.Completed => "#475569",
            BookingStatus.Cancelled => "#991B1B",
            BookingStatus.NoShow => "#991B1B",
            _ => "#475569"
        };

        // ─── Deposit ──────────────────────────────────────────────────────────

        public bool HasDeposit => _booking.HasDeposit;
        public decimal DepositAmount => _booking.DepositAmount;
        public string DepositMethod => _booking.DepositMethod;
        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        public string DepositDisplay => _booking.HasDeposit
            ? $"{CurrencySymbol}{_booking.DepositAmount:N2} deposit ({_booking.DepositMethod})"
            : "No deposit";

        // ─── Action Visibility ────────────────────────────────────────────────

        /// <summary>Show Confirm button only when Pending.</summary>
        public bool CanConfirm => _booking.Status == BookingStatus.Pending;

        /// <summary>Show Seat Now button when Pending or Confirmed.</summary>
        public bool CanSeat => _booking.CanBeSat;

        /// <summary>Show Cancel button when not already closed.</summary>
        public bool CanCancel =>
            _booking.Status != BookingStatus.Cancelled &&
            _booking.Status != BookingStatus.Completed &&
            _booking.Status != BookingStatus.NoShow;

        /// <summary>Show No Show button when Pending or Confirmed.</summary>
        public bool CanMarkNoShow =>
            _booking.Status == BookingStatus.Pending ||
            _booking.Status == BookingStatus.Confirmed;

        /// <summary>Show Edit button when not closed.</summary>
        public bool CanEdit =>
            _booking.Status != BookingStatus.Cancelled &&
            _booking.Status != BookingStatus.Completed &&
            _booking.Status != BookingStatus.NoShow &&
            _booking.Status != BookingStatus.Seated;

        // ─── Events (parent VM listens to these) ──────────────────────────────

        /// <summary>
        /// Fired when the customer is ready to be seated.
        /// Payload is the booking ID — parent handles navigation.
        /// </summary>
        public event Action<BookingSlotViewModel>? SeatRequested;

        /// <summary>Fired when the edit button is tapped.</summary>
        public event Action<BookingSlotViewModel>? EditRequested;

        /// <summary>Fired after any status change so the parent can refresh.</summary>
        public event Action? StateChanged;

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand ConfirmCommand { get; }
        public ICommand SeatCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand NoShowCommand { get; }
        public ICommand EditCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public BookingSlotViewModel(
            Booking booking,
            IBookingService bookingService,
            SettingsService settingsService)
        {
            _booking = booking;
            _bookingService = bookingService;
            _settingsService = settingsService;

            ConfirmCommand = new RelayCommand(
                async () => await ConfirmAsync(),
                () => CanConfirm);

            SeatCommand = new RelayCommand(
                () => SeatRequested?.Invoke(this),
                () => CanSeat);

            CancelCommand = new RelayCommand(
                async () => await CancelAsync(),
                () => CanCancel);

            NoShowCommand = new RelayCommand(
                async () => await MarkNoShowAsync(),
                () => CanMarkNoShow);

            EditCommand = new RelayCommand(
                () => EditRequested?.Invoke(this),
                () => CanEdit);

            // When currency symbol changes, refresh deposit display
            _settingsService.SettingsChanged += () =>
            {
                OnPropertyChanged(nameof(CurrencySymbol));
                OnPropertyChanged(nameof(DepositDisplay));
            };
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Replaces the internal booking snapshot with a refreshed one.
        /// Called by the parent after an edit is saved.
        /// </summary>
        public void UpdateFrom(Booking booking)
        {
            _booking = booking;
            RaiseAll();
        }

        // ─── Action Handlers ──────────────────────────────────────────────────

        private async Task ConfirmAsync()
        {
            try
            {
                await _bookingService.ConfirmBookingAsync(_booking.Id);
                _booking.Status = BookingStatus.Confirmed;
                RaiseAll();
                StateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private async Task CancelAsync()
        {
            // Ask if deposit should be refunded (only relevant when deposit exists)
            bool refundDeposit = false;

            if (_booking.HasDeposit)
            {
                var result = System.Windows.MessageBox.Show(
                    $"A deposit of {CurrencySymbol}{_booking.DepositAmount:N2} was paid.\n\n" +
                    "Click YES to refund the deposit.\n" +
                    "Click NO to forfeit it.",
                    "Deposit Refund?",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                refundDeposit = result == System.Windows.MessageBoxResult.Yes;
            }
            else
            {
                // No deposit — just confirm cancellation
                var result = System.Windows.MessageBox.Show(
                    $"Cancel booking for {_booking.CustomerName}?\nThis cannot be undone.",
                    "Cancel Booking",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result != System.Windows.MessageBoxResult.Yes)
                    return;
            }

            try
            {
                await _bookingService.CancelBookingAsync(_booking.Id, refundDeposit);
                _booking.Status = BookingStatus.Cancelled;
                RaiseAll();
                StateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private async Task MarkNoShowAsync()
        {
            var result = System.Windows.MessageBox.Show(
                $"Mark {_booking.CustomerName} as a no-show?\n\n" +
                (_booking.HasDeposit
                    ? $"The deposit of {CurrencySymbol}{_booking.DepositAmount:N2} will be forfeited."
                    : "No deposit was recorded."),
                "No Show",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                await _bookingService.MarkNoShowAsync(_booking.Id);
                _booking.Status = BookingStatus.NoShow;
                RaiseAll();
                StateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // ─── Private Helpers ──────────────────────────────────────────────────

        private void RaiseAll()
        {
            // Identity
            OnPropertyChanged(nameof(CustomerName));
            OnPropertyChanged(nameof(CustomerPhone));
            OnPropertyChanged(nameof(PartySize));
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(TableLabel));
            OnPropertyChanged(nameof(HasTable));

            // Time
            OnPropertyChanged(nameof(BookingTime));
            OnPropertyChanged(nameof(BookingDateDisplay));
            OnPropertyChanged(nameof(TimeUntilDisplay));

            // Status
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusLabel));
            OnPropertyChanged(nameof(StatusColour));
            OnPropertyChanged(nameof(StatusTextColour));

            // Deposit
            OnPropertyChanged(nameof(HasDeposit));
            OnPropertyChanged(nameof(DepositDisplay));

            // Action visibility
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(CanSeat));
            OnPropertyChanged(nameof(CanCancel));
            OnPropertyChanged(nameof(CanMarkNoShow));
            OnPropertyChanged(nameof(CanEdit));

            // Command CanExecute
            ((RelayCommand)ConfirmCommand).NotifyCanExecuteChanged();
            ((RelayCommand)SeatCommand).NotifyCanExecuteChanged();
            ((RelayCommand)CancelCommand).NotifyCanExecuteChanged();
            ((RelayCommand)NoShowCommand).NotifyCanExecuteChanged();
            ((RelayCommand)EditCommand).NotifyCanExecuteChanged();
        }

        private static void ShowError(string message) =>
            System.Windows.MessageBox.Show(
                message, "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
    }
}
