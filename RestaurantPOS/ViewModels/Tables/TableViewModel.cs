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
        private readonly ITableSessionService _tableSession;

        public int Number { get; }

        public bool HasOrder => _orderStore.HasOrder(Number);

        public bool IsCurrent => _tableSession.CurrentTable == Number;

        public bool IsLocked => HasOrder && !IsCurrent;

        public ICommand SelectTableCommand { get; }

        public TableViewModel(
            int number,
            ITableSessionService tableSession,
            OrderStore orderStore)
        {
            Number = number;

            _tableSession = tableSession;
            _orderStore = orderStore;

            SelectTableCommand = new RelayCommand(SelectTable, CanSelectTable);

            _orderStore.OrderStateChanged += OnOrderStateChanged;
            _tableSession.TableChanged += _ => RaiseAll();
        }

        private bool CanSelectTable()
        => !IsLocked;

        private void SelectTable()
            => _tableSession.SwitchTable(Number);

        private void OnOrderStateChanged(int tableNumber)
        {
            if (tableNumber == Number)
                RaiseAll();
        }

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(HasOrder));
            OnPropertyChanged(nameof(IsLocked));
            OnPropertyChanged(nameof(IsCurrent));
            ((RelayCommand)SelectTableCommand).NotifyCanExecuteChanged();
        }
    }
}
