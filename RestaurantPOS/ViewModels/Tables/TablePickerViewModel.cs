using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Tables
{
    public class TablePickerViewModel : ViewModelBase
    {
        private readonly IOrderContextService _tableSession;
        private readonly OrderStore _orderStore;

        public ObservableCollection<TableViewModel> Tables => _tableStore.Tables;

        private readonly TableStore _tableStore;

        public ICommand SelectTableCommand { get; }

        public TablePickerViewModel(
            IOrderContextService tableSession,
            OrderStore orderStore,
            TableStore tableStore)
        {
            _tableSession = tableSession;
            _orderStore = orderStore;
            _tableStore = tableStore;
            _ = LoadTables();

            SelectTableCommand = new RelayCommand<TableViewModel>(SelectTable);
        }

        private async Task LoadTables()
        {
            await _tableStore.LoadAsync();
        }

        private void SelectTable(TableViewModel table)
        {
            _tableSession.SwitchContext(table.tableNumber);
        }
    }

}
