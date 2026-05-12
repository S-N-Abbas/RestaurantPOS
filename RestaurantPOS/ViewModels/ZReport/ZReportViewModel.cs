using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Repositories;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.ZReport
{
    public class ZReportViewModel : ViewModelBase
    {
        private readonly IZReportService _reportService;
        private readonly ZReportBuilder _reportBuilder;
        private readonly SettingsService _settingsService;

        // ─── Date range ───────────────────────────────────────────────────────

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

        // ─── Quick range shortcuts ────────────────────────────────────────────

        public ICommand SetTodayCommand { get; }
        public ICommand SetYesterdayCommand { get; }
        public ICommand SetThisWeekCommand { get; }
        public ICommand SetThisMonthCommand { get; }

        // ─── Report state ─────────────────────────────────────────────────────

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _hasReport;
        public bool HasReport
        {
            get => _hasReport;
            set => SetProperty(ref _hasReport, value);
        }

        private ZReportData? _reportData;

        // The generated FlowDocument — bound to a DocumentViewer in the view
        private FlowDocument? _document;
        public FlowDocument? Document
        {
            get => _document;
            set => SetProperty(ref _document, value);
        }

        // ─── Summary figures for the on-screen header ─────────────────────────
        // These let staff see the numbers without printing

        public string TotalOrdersDisplay => _reportData?.Overall.OrderCount.ToString() ?? "—";
        public string TotalIncomeDisplay => _reportData != null
            ? $"{_currency}{_reportData.Overall.GrandTotal:N2}" : "—";
        public string CashIncomeDisplay => _reportData != null
            ? $"{_currency}{_reportData.Overall.CashTotal:N2}" : "—";
        public string CardIncomeDisplay => _reportData != null
            ? $"{_currency}{_reportData.Overall.CardTotal:N2}" : "—";
        public string CancellationsDisplay => _reportData?.Overall.CancelCount.ToString() ?? "—";

        private readonly string _currency;

        // ─── Commands ─────────────────────────────────────────────────────────

        public ICommand GenerateCommand { get; }
        public ICommand PrintCommand { get; }

        // ─── Constructor ──────────────────────────────────────────────────────

        public ZReportViewModel(
            IZReportService reportService,
            ZReportBuilder reportBuilder,
            SettingsService settingsService)
        {
            _reportService = reportService;
            _reportBuilder = reportBuilder;
            _settingsService = settingsService;
            _currency = _settingsService.Settings.CurrencySymbol;

            // Quick range commands
            SetTodayCommand = new RelayCommand(() => SetRange(DateTime.Today, DateTime.Today.AddDays(1).AddSeconds(-1)));
            SetYesterdayCommand = new RelayCommand(() => SetRange(DateTime.Today.AddDays(-1), DateTime.Today.AddSeconds(-1)));
            SetThisWeekCommand = new RelayCommand(() =>
            {
                var start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                SetRange(start, DateTime.Now);
            });
            SetThisMonthCommand = new RelayCommand(() =>
            {
                var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                SetRange(start, DateTime.Now);
            });

            GenerateCommand = new RelayCommand(async () => await GenerateAsync());
            PrintCommand = new RelayCommand(Print, () => HasReport);
        }

        // ─── Logic ────────────────────────────────────────────────────────────

        private void SetRange(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        private async Task GenerateAsync()
        {
            if (From > To)
            {
                System.Windows.MessageBox.Show(
                    "From date must be before To date.",
                    "Invalid Range",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            HasReport = false;
            Document = null;

            try
            {
                _reportData = await _reportService.GenerateAsync(From, To);
                Document = _reportBuilder.Build(_reportData);
                HasReport = true;

                // Raise all summary displays
                OnPropertyChanged(nameof(TotalOrdersDisplay));
                OnPropertyChanged(nameof(TotalIncomeDisplay));
                OnPropertyChanged(nameof(CashIncomeDisplay));
                OnPropertyChanged(nameof(CardIncomeDisplay));
                OnPropertyChanged(nameof(CancellationsDisplay));

                ((RelayCommand)PrintCommand).NotifyCanExecuteChanged();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Print()
        {
            if (Document == null) return;

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
                        ((IDocumentPaginatorSource)Document).DocumentPaginator,
                        "Z-Report");

                    return;
                }
                catch (Exception ex)
                {
                    // Printer not found or unavailable — fall through to dialog
                    System.Diagnostics.Debug.WriteLine(
                        $"Default printer '{printerName}' unavailable: {ex.Message}");
                }

                // ✅ Fallback — show dialog if no default printer or it failed
                var fallbackDialog = new PrintDialog();
                if (fallbackDialog.ShowDialog() == true)
                {
                    fallbackDialog.PrintDocument(
                        ((IDocumentPaginatorSource)Document).DocumentPaginator,
                        "Z-Report");
                }
            }
        }
    }
}
