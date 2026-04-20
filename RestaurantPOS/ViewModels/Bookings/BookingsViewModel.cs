using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Bookings
{
    public class BookingsViewModel : ViewModelBase
    {
        private readonly IBookingService _bookingService;
        private readonly ITableService _tableService;
        private readonly IOrderContextService _orderContextService;
        private readonly OrderStore _orderStore;
        private readonly INavigationService _navigationService;
        private readonly SettingsService _settingsService;

        // ─── Sub ViewModels ───────────────────────────────────────────────────

        public BookingEditorViewModel Editor { get; }

        // ─── Booking List ─────────────────────────────────────────────────────

        public ObservableCollection<BookingSlotViewModel> Bookings { get; } = new();

        // ─── Date Navigation ──────────────────────────────────────────────────

        private DateOnly _viewingDate = DateOnly.FromDateTime(DateTime.Today);
        public DateOnly ViewingDate
        {
            get => _viewingDate;
            private set
            {
                if (SetProperty(ref _viewingDate, value))
                {
                    OnPropertyChanged(nameof(ViewingDateDisplay));
                    OnPropertyChanged(nameof(IsViewingToday));
                    _ = LoadBookingsAsync();
                }
            }
        }

        public string ViewingDateDisplay => _viewingDate == DateOnly.FromDateTime(DateTime.Today)
            ? $"Today — {_viewingDate:dddd d MMMM yyyy}"
            : _viewingDate.ToString("dddd d MMMM yyyy");

        public bool IsViewingToday
            => _viewingDate == DateOnly.FromDateTime(DateTime.Today);

        // ─── Loading State ────────────────────────────────────────────────────

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

        public string EmptyMessage => IsViewingToday
            ? "No bookings for today."
            : $"No bookings for {_viewingDate:d MMMM}.";

        // ─── Seat Picker State ────────────────────────────────────────────────
        // When the user taps "Seat Now", we need them to pick a table
        // if one wasn't pre-assigned. This overlay shows a simple table grid.

        private bool _isSeatPickerOpen;
        public bool IsSeatPickerOpen
        {
            get => _isSeatPickerOpen;
            set => SetProperty(ref _isSeatPickerOpen, value);
        }

        private BookingSlotViewModel? _pendingSeat;

        public ObservableCollection<SeatTableSlotViewModel> AvailableTables { get; } = new();

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand NewBookingCommand { get; }
        public ICommand PreviousDayCommand { get; }
        public ICommand NextDayCommand { get; }
        public ICommand TodayCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SelectSeatTableCommand { get; }
        public ICommand CloseSeatPickerCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public BookingsViewModel(
            IBookingService bookingService,
            ITableService tableService,
            IOrderContextService orderContextService,
            OrderStore orderStore,
            INavigationService navigationService,
            SettingsService settingsService)
        {
            _bookingService = bookingService;
            _tableService = tableService;
            _orderContextService = orderContextService;
            _orderStore = orderStore;
            _navigationService = navigationService;
            _settingsService = settingsService;

            // ── Editor ─────────────────────────────────────────────────────────
            Editor = new BookingEditorViewModel(
                bookingService, tableService, settingsService);

            Editor.SavedSuccessfully += OnEditorSaved;

            // ── Commands ───────────────────────────────────────────────────────
            NewBookingCommand = new RelayCommand(()
                => Editor.OpenForNew());

            PreviousDayCommand = new RelayCommand(()
                => ViewingDate = ViewingDate.AddDays(-1));

            NextDayCommand = new RelayCommand(()
                => ViewingDate = ViewingDate.AddDays(1));

            TodayCommand = new RelayCommand(()
                => ViewingDate = DateOnly.FromDateTime(DateTime.Today));

            RefreshCommand = new RelayCommand(async ()
                => await LoadBookingsAsync());

            SelectSeatTableCommand = new RelayCommand<SeatTableSlotViewModel>(
                async slot => await OnSeatTableSelectedAsync(slot));

            CloseSeatPickerCommand = new RelayCommand(() =>
            {
                IsSeatPickerOpen = false;
                _pendingSeat = null;
            });

            _ = LoadBookingsAsync();
        }

        // ─── Load ─────────────────────────────────────────────────────────────

        private async Task LoadBookingsAsync()
        {
            IsLoading = true;
            IsEmpty = false;

            try
            {
                var bookings = await _bookingService.GetBookingsByDateAsync(_viewingDate);

                // Unsubscribe old slot VMs to prevent event leaks
                foreach (var slot in Bookings)
                    UnwireSlot(slot);

                Bookings.Clear();

                foreach (var booking in bookings)
                {
                    var slot = CreateSlotViewModel(booking);
                    Bookings.Add(slot);
                }

                IsEmpty = !Bookings.Any();
                OnPropertyChanged(nameof(EmptyMessage));
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ─── Slot VM Factory ──────────────────────────────────────────────────

        private BookingSlotViewModel CreateSlotViewModel(Booking booking)
        {
            var slot = new BookingSlotViewModel(
                booking, _bookingService, _settingsService);

            // ── Wire events ───────────────────────────────────────────────────
            slot.SeatRequested += OnSeatRequested;
            slot.EditRequested += OnEditRequested;
            slot.StateChanged += async () => await OnSlotStateChangedAsync();

            return slot;
        }

        private void UnwireSlot(BookingSlotViewModel slot)
        {
            slot.SeatRequested -= OnSeatRequested;
            slot.EditRequested -= OnEditRequested;
        }

        // ─── Editor Callbacks ─────────────────────────────────────────────────

        private void OnEditorSaved(Booking saved)
        {
            // Try to find an existing slot VM for this booking
            var existing = Bookings.FirstOrDefault(s => s.Id == saved.Id);

            if (existing != null)
            {
                // Update existing card in place — no flicker
                existing.UpdateFrom(saved);
            }
            else
            {
                // New booking — only add to list if it belongs to the viewed date
                if (DateOnly.FromDateTime(saved.BookingDate) == _viewingDate)
                {
                    var slot = CreateSlotViewModel(saved);

                    // Insert in time order
                    var insertIndex = Bookings
                        .TakeWhile(s => s.BookingTime.CompareTo(slot.BookingTime) <= 0)
                        .Count();

                    Bookings.Insert(insertIndex, slot);
                }
            }

            IsEmpty = !Bookings.Any();
        }

        private void OnEditRequested(BookingSlotViewModel slot)
        {
            // Load the full booking from the slot and open editor
            _ = OpenEditorForSlotAsync(slot);
        }

        private async Task OpenEditorForSlotAsync(BookingSlotViewModel slot)
        {
            try
            {
                // Reload fresh from DB to get navigation properties
                var booking = await _bookingService.GetByIdAsync(slot.Id);
                Editor.OpenForEdit(booking);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private async Task OnSlotStateChangedAsync()
        {
            // After cancel / no-show / confirm — check if list needs
            // filtering (e.g. cancelled bookings can still be shown)
            // We do a lightweight refresh rather than full reload
            IsEmpty = !Bookings.Any();
            await Task.CompletedTask;
        }

        // ─── Seating Flow ─────────────────────────────────────────────────────

        private void OnSeatRequested(BookingSlotViewModel slot)
        {
            _pendingSeat = slot;

            // If the booking already has a pre-assigned table, seat immediately
            // without showing the picker — but only if that table is free
            _ = AttemptAutoSeatAsync(slot);
        }

        private async Task AttemptAutoSeatAsync(BookingSlotViewModel slot)
        {
            try
            {
                var booking = await _bookingService.GetByIdAsync(slot.Id);

                if (booking.TableId.HasValue)
                {
                    // Pre-assigned table — check if it's free
                    var tableNumber = booking.Table!.Number;
                    bool occupied = _orderStore.HasOrder(tableNumber);

                    if (!occupied)
                    {
                        // Seat directly
                        await SeatOnTableAsync(slot, tableNumber);
                        return;
                    }

                    // Table is occupied — fall through to picker with a warning
                    ShowWarning(
                        $"Table {tableNumber} (pre-assigned) is currently occupied.\n" +
                        "Please choose a different table.");
                }

                // No pre-assigned table or it's occupied — show picker
                await BuildAvailableTablesAsync();
                IsSeatPickerOpen = true;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private async Task BuildAvailableTablesAsync()
        {
            var allTables = await _tableService.GetAllAsync();

            AvailableTables.Clear();

            foreach (var table in allTables.OrderBy(t => t.Number))
            {
                AvailableTables.Add(new SeatTableSlotViewModel(
                    table.Number,
                    _orderStore.HasOrder(table.Number)));
            }
        }

        private async Task OnSeatTableSelectedAsync(SeatTableSlotViewModel? slot)
        {
            if (slot == null || slot.IsOccupied || _pendingSeat == null)
                return;

            IsSeatPickerOpen = false;

            await SeatOnTableAsync(_pendingSeat, slot.TableNumber);

            _pendingSeat = null;
        }

        private async Task SeatOnTableAsync(BookingSlotViewModel slot, int tableNumber)
        {
            try
            {
                // ── 1. Service creates the order + pre-records deposit ─────────
                var order = await _bookingService.SeatBookingAsync(slot.Id, tableNumber);

                // ── 2. Register order in the in-memory store ──────────────────
                var orderState = new OrderState(
                    tableNumber,
                    order,
                    _settingsService,
                    _ => { }); // removeCallback — OrderViewModel will replace this

                _orderStore.RegisterOrder(tableNumber, orderState);

                // ── 3. Switch context to the table ────────────────────────────
                _orderContextService.SwitchContext(tableNumber);

                // ── 4. Navigate to OrderView ──────────────────────────────────
                _navigationService.NavigateTo<OrderViewModel>();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static void ShowError(string message) =>
            System.Windows.MessageBox.Show(
                message, "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

        private static void ShowWarning(string message) =>
            System.Windows.MessageBox.Show(
                message, "Table Unavailable",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
    }
}
