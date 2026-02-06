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
        private readonly ITableSessionService _tableSession;
        private readonly OrderStore _orderStore;

        public ObservableCollection<TableViewModel> Tables { get; }

        public ICommand SelectTableCommand { get; }

        public TablePickerViewModel(
            ITableSessionService tableSession,
            OrderStore orderStore)
        {
            _tableSession = tableSession;
            _orderStore = orderStore;

            Tables = new ObservableCollection<TableViewModel>();

            for (int i = 1; i <= 12; i++)
            {
                var table = new TableViewModel(i, tableSession, orderStore)
                {
                    HasOrder = _orderStore.GetOrCreate(i).Items.Any(),
                };
                Tables.Add(table);
            }

            SelectTableCommand = new RelayCommand<TableViewModel>(SelectTable);
        }

        private void SelectTable(TableViewModel table)
        {
            _tableSession.SwitchTable(table.Number);
        }
    }

}
