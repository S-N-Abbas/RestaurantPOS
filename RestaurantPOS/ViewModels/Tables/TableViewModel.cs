using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace RestaurantPOS.ViewModels.Tables
{
    public class TableViewModel : ViewModelBase
    {
        private readonly OrderStore _orderStore;
        private readonly IOrderContextService _orderContextService;

        public int tableNumber { get; }

        public bool HasOrder => _orderStore.HasOrder(tableNumber);

        public bool IsCurrent => _orderContextService.CurrentContext == tableNumber;

        public bool IsLocked => HasOrder && !IsCurrent;

        public decimal CurrentTotal => _orderStore.GetOrderTotal(tableNumber);

        public ICommand SelectTableCommand { get; }

        public TableViewModel(
            int number,
            IOrderContextService orderContextService,
            OrderStore orderStore)
        {
            tableNumber = number;

            _orderContextService = orderContextService;
            _orderStore = orderStore;

            SelectTableCommand = new RelayCommand(SelectTable, CanSelectTable);

            _orderStore.OrderStateChanged += OnOrderStateChanged;
            _orderContextService.ContextChanged += _ => RaiseAll();
        }

        private bool CanSelectTable()
        => !IsLocked;

        private void SelectTable()
            => _orderContextService.SwitchContext(tableNumber);

        private void OnOrderStateChanged(int tableNumber)
        {
            if (tableNumber == this.tableNumber)
                RaiseAll();
        }

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(HasOrder));
            OnPropertyChanged(nameof(IsLocked));
            OnPropertyChanged(nameof(IsCurrent));
            OnPropertyChanged(nameof(CurrentTotal));
            ((RelayCommand)SelectTableCommand).NotifyCanExecuteChanged();
        }
    }
}
