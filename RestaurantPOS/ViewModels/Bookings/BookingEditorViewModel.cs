using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Bookings
{
    // ─── Which text field the keyboard is currently targeting ─────────────────
    public enum BookingEditorField
    {
        CustomerName,
        CustomerPhone,
        CustomerEmail,
        Notes
    }

    public class BookingEditorViewModel : ViewModelBase
    {
        private readonly IBookingService _bookingService;
        private readonly ITableService _tableService;
        private readonly SettingsService _settingsService;

        // ─── Mode ─────────────────────────────────────────────────────────────

        private int? _editingId;

        public string Title => _editingId == null ? "New Booking" : "Edit Booking";

        public bool IsNewBooking => _editingId == null;

        // ─── Visibility ───────────────────────────────────────────────────────

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        // ─── Customer Fields ──────────────────────────────────────────────────

        private string _customerName = string.Empty;
        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        private string _customerPhone = string.Empty;
        public string CustomerPhone
        {
            get => _customerPhone;
            set => SetProperty(ref _customerPhone, value);
        }

        private string _customerEmail = string.Empty;
        public string CustomerEmail
        {
            get => _customerEmail;
            set => SetProperty(ref _customerEmail, value);
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        // ─── Party Size (numpad driven) ───────────────────────────────────────

        private int _partySize = 2;
        public int PartySize
        {
            get => _partySize;
            set => SetProperty(ref _partySize, Math.Max(1, value));
        }

        // ─── Date & Time (stepper driven — touch friendly) ───────────────────

        private DateTime _bookingDate = DateTime.Today.AddDays(1).AddHours(19); // default: tomorrow 7pm

        public int BookingDay
        {
            get => _bookingDate.Day;
        }

        public int BookingMonth
        {
            get => _bookingDate.Month;
        }

        public int BookingYear
        {
            get => _bookingDate.Year;
        }

        public int BookingHour
        {
            get => _bookingDate.Hour;
        }

        public int BookingMinute
        {
            get => _bookingDate.Minute;
        }

        /// <summary>Full formatted display — e.g. "Saturday 19 April 2026 at 7:00 PM"</summary>
        public string BookingDateDisplay
            => _bookingDate.ToString("dddd d MMMM yyyy 'at' h:mm tt");

        // ─── Date steppers ────────────────────────────────────────────────────

        public ICommand IncreaseDayCommand { get; }
        public ICommand DecreaseDayCommand { get; }
        public ICommand IncreaseMonthCommand { get; }
        public ICommand DecreaseMonthCommand { get; }
        public ICommand IncreaseYearCommand { get; }
        public ICommand DecreaseYearCommand { get; }
        public ICommand IncreaseHourCommand { get; }
        public ICommand DecreaseHourCommand { get; }
        public ICommand IncreaseMinuteCommand { get; }
        public ICommand DecreaseMinuteCommand { get; }

        // ─── Table Picker ─────────────────────────────────────────────────────

        public ObservableCollection<Table> Tables { get; } = new();

        private Table? _selectedTable;
        public Table? SelectedTable
        {
            get => _selectedTable;
            set => SetProperty(ref _selectedTable, value);
        }

        /// <summary>Convenience label for the selected table.</summary>
        public string TableDisplay => _selectedTable != null
            ? $"Table {_selectedTable.Number}"
            : "No table (assign later)";

        // ─── Deposit Section ──────────────────────────────────────────────────

        private bool _takeDeposit;
        public bool TakeDeposit
        {
            get => _takeDeposit;
            set
            {
                if (SetProperty(ref _takeDeposit, value))
                {
                    // Clear deposit fields when toggled off
                    if (!value)
                    {
                        DepositAmountDisplay = "0.00";
                        SelectedDepositMethod = DepositMethods.First();
                    }
                    OnPropertyChanged(nameof(IsDepositSectionVisible));
                }
            }
        }

        public bool IsDepositSectionVisible => TakeDeposit;

        /// <summary>
        /// True when editing an existing booking that already has a paid deposit.
        /// In this case the deposit section is read-only.
        /// </summary>
        public bool IsDepositAlreadyPaid { get; private set; }

        // Deposit amount — numpad driven, stored as display string
        private string _depositAmountDisplay = "0.00";
        public string DepositAmountDisplay
        {
            get => _depositAmountDisplay;
            set => SetProperty(ref _depositAmountDisplay, value);
        }

        public decimal DepositAmount =>
            decimal.TryParse(_depositAmountDisplay, out var d) ? d : 0m;

        public IEnumerable<string> DepositMethods { get; } =
            new[] { "Cash", "Card" };

        private string _selectedDepositMethod = "Cash";
        public string SelectedDepositMethod
        {
            get => _selectedDepositMethod;
            set => SetProperty(ref _selectedDepositMethod, value);
        }

        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        // ─── Active field tracking (keyboard target) ──────────────────────────

        private BookingEditorField _activeField = BookingEditorField.CustomerName;

        public bool IsCustomerNameActive => _activeField == BookingEditorField.CustomerName;
        public bool IsCustomerPhoneActive => _activeField == BookingEditorField.CustomerPhone;
        public bool IsCustomerEmailActive => _activeField == BookingEditorField.CustomerEmail;
        public bool IsNotesActive => _activeField == BookingEditorField.Notes;

        // ─── Active numpad target (PartySize or Deposit) ──────────────────────

        private bool _numpadTargetsDeposit;

        public bool NumpadTargetsPartySize => !_numpadTargetsDeposit;
        public bool NumpadTargetsDeposit => _numpadTargetsDeposit;

        // ─── Events ───────────────────────────────────────────────────────────

        /// <summary>Fired after a successful save. Payload is the saved booking.</summary>
        public event Action<Booking>? SavedSuccessfully;

        public event Action? Cancelled;

        // ─── Commands ─────────────────────────────────────────────────────────

        // Keyboard
        public ICommand FocusCustomerNameCommand { get; }
        public ICommand FocusCustomerPhoneCommand { get; }
        public ICommand FocusCustomerEmailCommand { get; }
        public ICommand FocusNotesCommand { get; }
        public ICommand KeyCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearFieldCommand { get; }

        // Numpad (shared between PartySize and Deposit)
        public ICommand NumpadFocusPartySizeCommand { get; }
        public ICommand NumpadFocusDepositCommand { get; }
        public ICommand NumpadKeyCommand { get; }
        public ICommand NumpadBackspaceCommand { get; }
        public ICommand NumpadClearCommand { get; }

        // Party size steppers
        public ICommand IncreasePartySizeCommand { get; }
        public ICommand DecreasePartySizeCommand { get; }

        // Table selection
        public ICommand SelectTableCommand { get; }
        public ICommand ClearTableCommand { get; }

        // Form
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public BookingEditorViewModel(
            IBookingService bookingService,
            ITableService tableService,
            SettingsService settingsService)
        {
            _bookingService = bookingService;
            _tableService = tableService;
            _settingsService = settingsService;

            // ── Date steppers ──────────────────────────────────────────────────
            IncreaseDayCommand = new RelayCommand(() => ShiftDate(days: 1));
            DecreaseDayCommand = new RelayCommand(() => ShiftDate(days: -1));
            IncreaseMonthCommand = new RelayCommand(() => ShiftDate(months: 1));
            DecreaseMonthCommand = new RelayCommand(() => ShiftDate(months: -1));
            IncreaseYearCommand = new RelayCommand(() => ShiftDate(years: 1));
            DecreaseYearCommand = new RelayCommand(() => ShiftDate(years: -1));
            IncreaseHourCommand = new RelayCommand(() => ShiftTime(hours: 1));
            DecreaseHourCommand = new RelayCommand(() => ShiftTime(hours: -1));
            IncreaseMinuteCommand = new RelayCommand(() => ShiftTime(minutes: 15));
            DecreaseMinuteCommand = new RelayCommand(() => ShiftTime(minutes: -15));

            // ── Keyboard ───────────────────────────────────────────────────────
            FocusCustomerNameCommand = new RelayCommand(()
                => SetActiveField(BookingEditorField.CustomerName));
            FocusCustomerPhoneCommand = new RelayCommand(()
                => SetActiveField(BookingEditorField.CustomerPhone));
            FocusCustomerEmailCommand = new RelayCommand(()
                => SetActiveField(BookingEditorField.CustomerEmail));
            FocusNotesCommand = new RelayCommand(()
                => SetActiveField(BookingEditorField.Notes));

            KeyCommand = new RelayCommand<string>(AppendKey);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearFieldCommand = new RelayCommand(ClearActiveField);

            // ── Numpad ─────────────────────────────────────────────────────────
            NumpadFocusPartySizeCommand = new RelayCommand(() => SetNumpadTarget(false));
            NumpadFocusDepositCommand = new RelayCommand(() => SetNumpadTarget(true));
            NumpadKeyCommand = new RelayCommand<string>(AppendNumpadKey);
            NumpadBackspaceCommand = new RelayCommand(NumpadBackspace);
            NumpadClearCommand = new RelayCommand(NumpadClear);

            // ── Party size ─────────────────────────────────────────────────────
            IncreasePartySizeCommand = new RelayCommand(() => PartySize++);
            DecreasePartySizeCommand = new RelayCommand(() => PartySize--);

            // ── Table ──────────────────────────────────────────────────────────
            SelectTableCommand = new RelayCommand<Table>(t =>
            {
                SelectedTable = t;
                OnPropertyChanged(nameof(TableDisplay));
            });

            ClearTableCommand = new RelayCommand(() =>
            {
                SelectedTable = null;
                OnPropertyChanged(nameof(TableDisplay));
            });

            // ── Form ───────────────────────────────────────────────────────────
            SaveCommand = new RelayCommand(async () => await SaveAsync());
            CancelCommand = new RelayCommand(Close);

            _settingsService.SettingsChanged += () =>
                OnPropertyChanged(nameof(CurrencySymbol));

            _ = LoadTablesAsync();
        }

        // ─── Open API ─────────────────────────────────────────────────────────

        /// <summary>Opens the editor in New Booking mode.</summary>
        public void OpenForNew()
        {
            _editingId = null;
            IsDepositAlreadyPaid = false;
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerEmail = string.Empty;
            Notes = string.Empty;
            PartySize = 2;
            _bookingDate = DateTime.Today.AddDays(1).AddHours(19);
            SelectedTable = null;
            TakeDeposit = false;
            DepositAmountDisplay = "0.00";
            SelectedDepositMethod = "Cash";
            _activeField = BookingEditorField.CustomerName;
            _numpadTargetsDeposit = false;

            RaiseAll();
            IsOpen = true;
        }

        /// <summary>Opens the editor pre-filled with an existing booking.</summary>
        public void OpenForEdit(Booking booking)
        {
            _editingId = booking.Id;
            IsDepositAlreadyPaid = booking.DepositPaid;
            CustomerName = booking.CustomerName;
            CustomerPhone = booking.CustomerPhone;
            CustomerEmail = booking.CustomerEmail;
            Notes = booking.Notes;
            PartySize = booking.PartySize;
            _bookingDate = booking.BookingDate;
            SelectedTable = Tables.FirstOrDefault(t => t.Id == booking.TableId);
            TakeDeposit = booking.DepositPaid;
            DepositAmountDisplay = booking.DepositAmount.ToString("N2");
            SelectedDepositMethod = string.IsNullOrWhiteSpace(booking.DepositMethod)
                                     ? "Cash"
                                     : booking.DepositMethod;
            _activeField = BookingEditorField.CustomerName;
            _numpadTargetsDeposit = false;

            RaiseAll();
            IsOpen = true;
        }

        // ─── Date / Time Steppers ─────────────────────────────────────────────

        private void ShiftDate(int days = 0, int months = 0, int years = 0)
        {
            try
            {
                _bookingDate = _bookingDate
                    .AddDays(days)
                    .AddMonths(months)
                    .AddYears(years);

                // Never allow past dates
                if (_bookingDate < DateTime.Now)
                    _bookingDate = DateTime.Now.AddMinutes(30);

                RaiseDateProperties();
            }
            catch
            {
                // AddMonths can throw on invalid day — clamp to end of month
                _bookingDate = new DateTime(
                    _bookingDate.Year,
                    _bookingDate.Month,
                    1).AddMonths(months)
                    .AddYears(years);
                RaiseDateProperties();
            }
        }

        private void ShiftTime(int hours = 0, int minutes = 0)
        {
            _bookingDate = _bookingDate.AddHours(hours).AddMinutes(minutes);
            RaiseDateProperties();
        }

        private void RaiseDateProperties()
        {
            OnPropertyChanged(nameof(BookingDay));
            OnPropertyChanged(nameof(BookingMonth));
            OnPropertyChanged(nameof(BookingYear));
            OnPropertyChanged(nameof(BookingHour));
            OnPropertyChanged(nameof(BookingMinute));
            OnPropertyChanged(nameof(BookingDateDisplay));
        }

        // ─── Keyboard Logic ───────────────────────────────────────────────────

        private void SetActiveField(BookingEditorField field)
        {
            _activeField = field;
            RaiseFieldFocus();
        }

        private void RaiseFieldFocus()
        {
            OnPropertyChanged(nameof(IsCustomerNameActive));
            OnPropertyChanged(nameof(IsCustomerPhoneActive));
            OnPropertyChanged(nameof(IsCustomerEmailActive));
            OnPropertyChanged(nameof(IsNotesActive));
        }

        private void AppendKey(string? key)
        {
            if (key == null) return;

            switch (_activeField)
            {
                case BookingEditorField.CustomerName:
                    CustomerName += key;
                    break;
                case BookingEditorField.CustomerPhone:
                    // Phone — digits, spaces, + and - only
                    if (char.IsDigit(key[0]) || key == " " || key == "+" || key == "-")
                        CustomerPhone += key;
                    break;
                case BookingEditorField.CustomerEmail:
                    CustomerEmail += key;
                    break;
                case BookingEditorField.Notes:
                    Notes += key;
                    break;
            }
        }

        private void Backspace()
        {
            switch (_activeField)
            {
                case BookingEditorField.CustomerName when CustomerName.Length > 0:
                    CustomerName = CustomerName[..^1]; break;
                case BookingEditorField.CustomerPhone when CustomerPhone.Length > 0:
                    CustomerPhone = CustomerPhone[..^1]; break;
                case BookingEditorField.CustomerEmail when CustomerEmail.Length > 0:
                    CustomerEmail = CustomerEmail[..^1]; break;
                case BookingEditorField.Notes when Notes.Length > 0:
                    Notes = Notes[..^1]; break;
            }
        }

        private void ClearActiveField()
        {
            switch (_activeField)
            {
                case BookingEditorField.CustomerName: CustomerName = string.Empty; break;
                case BookingEditorField.CustomerPhone: CustomerPhone = string.Empty; break;
                case BookingEditorField.CustomerEmail: CustomerEmail = string.Empty; break;
                case BookingEditorField.Notes: Notes = string.Empty; break;
            }
        }

        // ─── Numpad Logic (PartySize & Deposit) ───────────────────────────────

        private void SetNumpadTarget(bool targetsDeposit)
        {
            _numpadTargetsDeposit = targetsDeposit;
            OnPropertyChanged(nameof(NumpadTargetsPartySize));
            OnPropertyChanged(nameof(NumpadTargetsDeposit));
        }

        private void AppendNumpadKey(string? key)
        {
            if (key == null) return;

            if (_numpadTargetsDeposit)
            {
                // Decimal amount — allow one decimal point
                if (key == "." && DepositAmountDisplay.Contains('.')) return;
                if (key != "." && !char.IsDigit(key[0])) return;
                DepositAmountDisplay = DepositAmountDisplay == "0.00"
                    ? key
                    : DepositAmountDisplay + key;
            }
            else
            {
                // Party size — integers only
                if (!char.IsDigit(key[0])) return;
                int current = PartySize;
                int digit = int.Parse(key);
                PartySize = current == 0 ? digit : (current * 10) + digit;
            }
        }

        private void NumpadBackspace()
        {
            if (_numpadTargetsDeposit)
            {
                if (DepositAmountDisplay.Length > 1)
                    DepositAmountDisplay = DepositAmountDisplay[..^1];
                else
                    DepositAmountDisplay = "0.00";
            }
            else
            {
                PartySize /= 10;
                if (PartySize == 0) PartySize = 1;
            }
        }

        private void NumpadClear()
        {
            if (_numpadTargetsDeposit)
                DepositAmountDisplay = "0.00";
            else
                PartySize = 1;
        }

        // ─── Table Loading ────────────────────────────────────────────────────

        private async Task LoadTablesAsync()
        {
            var tables = await _tableService.GetAllAsync();
            Tables.Clear();
            foreach (var t in tables) Tables.Add(t);
        }

        // ─── Save ─────────────────────────────────────────────────────────────

        private async Task SaveAsync()
        {
            // ── Validation ────────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                ShowError("Customer name is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerPhone))
            {
                ShowError("Phone number is required.");
                return;
            }

            if (PartySize <= 0)
            {
                ShowError("Party size must be at least 1.");
                return;
            }

            if (_bookingDate <= DateTime.Now)
            {
                ShowError("Booking date must be in the future.");
                return;
            }

            if (TakeDeposit && !IsDepositAlreadyPaid && DepositAmount <= 0)
            {
                ShowError("Please enter a deposit amount greater than zero.");
                return;
            }

            try
            {
                Booking saved;

                if (_editingId == null)
                {
                    // ── Create ─────────────────────────────────────────────────
                    saved = await _bookingService.CreateBookingAsync(
                        customerName: CustomerName.Trim(),
                        customerPhone: CustomerPhone.Trim(),
                        customerEmail: CustomerEmail.Trim(),
                        partySize: PartySize,
                        bookingDate: _bookingDate,
                        tableId: SelectedTable?.Id,
                        notes: Notes.Trim(),
                        depositAmount: TakeDeposit ? DepositAmount : 0m,
                        depositPaid: TakeDeposit,
                        depositMethod: TakeDeposit ? SelectedDepositMethod : string.Empty);
                }
                else
                {
                    // ── Update ─────────────────────────────────────────────────
                    saved = await _bookingService.UpdateBookingAsync(
                        id: _editingId.Value,
                        customerName: CustomerName.Trim(),
                        customerPhone: CustomerPhone.Trim(),
                        customerEmail: CustomerEmail.Trim(),
                        partySize: PartySize,
                        bookingDate: _bookingDate,
                        tableId: SelectedTable?.Id,
                        notes: Notes.Trim());

                    // If deposit was newly added during edit (wasn't paid before)
                    if (TakeDeposit && !IsDepositAlreadyPaid && DepositAmount > 0)
                    {
                        await _bookingService.RecordDepositAsync(
                            _editingId.Value,
                            DepositAmount,
                            SelectedDepositMethod);
                    }
                }

                Close();
                SavedSuccessfully?.Invoke(saved);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Close()
        {
            IsOpen = false;
            Cancelled?.Invoke();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(IsNewBooking));
            OnPropertyChanged(nameof(CustomerName));
            OnPropertyChanged(nameof(CustomerPhone));
            OnPropertyChanged(nameof(CustomerEmail));
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(PartySize));
            OnPropertyChanged(nameof(SelectedTable));
            OnPropertyChanged(nameof(TableDisplay));
            OnPropertyChanged(nameof(TakeDeposit));
            OnPropertyChanged(nameof(IsDepositSectionVisible));
            OnPropertyChanged(nameof(IsDepositAlreadyPaid));
            OnPropertyChanged(nameof(DepositAmountDisplay));
            OnPropertyChanged(nameof(SelectedDepositMethod));
            OnPropertyChanged(nameof(IsDepositSectionVisible));
            RaiseDateProperties();
            RaiseFieldFocus();
            OnPropertyChanged(nameof(NumpadTargetsPartySize));
            OnPropertyChanged(nameof(NumpadTargetsDeposit));
        }

        private static void ShowError(string message) =>
            System.Windows.MessageBox.Show(
                message, "Validation",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
    }
}
