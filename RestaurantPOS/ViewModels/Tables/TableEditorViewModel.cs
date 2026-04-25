using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System.Windows.Input;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.ViewModels.Tables
{
    public class TableEditorViewModel : ViewModelBase
    {
        private readonly ITableService _tableService;

        // ─── Mode ─────────────────────────────────────────────────────────────

        private int? _editingId;
        private int? _editingCurrentNumber;

        public string Title => _editingId == null ? "Add Table" : "Edit Table";

        // ─── Visibility ───────────────────────────────────────────────────────

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        // ─── Number field ─────────────────────────────────────────────────────

        private string _numberDisplay = string.Empty;
        public string NumberDisplay
        {
            get => _numberDisplay;
            set
            {
                // Allow hardware keyboard — only accept digits
                var filtered = new string(value.Where(char.IsDigit).ToArray());
                if (SetProperty(ref _numberDisplay, filtered))
                    ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
            }
        }

        public int? TableNumber =>
            int.TryParse(_numberDisplay, out var n) && n > 0 ? n : null;

        // ─── Events ───────────────────────────────────────────────────────────

        public event Action<Table>? SavedSuccessfully;
        public event Action? Cancelled;

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand NumpadKeyCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public TableEditorViewModel(ITableService tableService)
        {
            _tableService = tableService;

            NumpadKeyCommand = new RelayCommand<string>(AppendDigit);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearCommand = new RelayCommand(() =>
            {
                NumberDisplay = string.Empty;
            });

            SaveCommand = new RelayCommand(
                async () => await SaveAsync(),
                () => TableNumber.HasValue);

            CancelCommand = new RelayCommand(Close);
        }

        // ─── Open API ─────────────────────────────────────────────────────────

        public void OpenForNew()
        {
            _editingId = null;
            _editingCurrentNumber = null;
            NumberDisplay = string.Empty;
            OnPropertyChanged(nameof(Title));
            IsOpen = true;
        }

        public void OpenForEdit(int tableId, int currentNumber)
        {
            _editingId = tableId;
            _editingCurrentNumber = currentNumber;
            NumberDisplay = currentNumber.ToString();
            OnPropertyChanged(nameof(Title));
            IsOpen = true;
        }

        // ─── Numpad ───────────────────────────────────────────────────────────

        private void AppendDigit(string? digit)
        {
            if (digit == null || !char.IsDigit(digit[0])) return;
            if (NumberDisplay.Length >= 3) return;   // max table number 999

            NumberDisplay += digit;
        }

        private void Backspace()
        {
            if (NumberDisplay.Length > 0)
                NumberDisplay = NumberDisplay[..^1];
        }

        // ─── Save ─────────────────────────────────────────────────────────────

        private async Task SaveAsync()
        {
            if (!TableNumber.HasValue) return;

            try
            {
                var saved = await _tableService.SaveTableAsync(
                    _editingId,
                    TableNumber.Value);

                Close();
                SavedSuccessfully?.Invoke(saved);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.Message, "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
        }

        private void Close()
        {
            IsOpen = false;
            Cancelled?.Invoke();
        }
    }
}