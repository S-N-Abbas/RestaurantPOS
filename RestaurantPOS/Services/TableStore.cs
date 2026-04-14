using RestaurantPOS.ViewModels.Tables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class TableStore
    {
        public ObservableCollection<TableViewModel> Tables { get; }

        private readonly ITableService _tableService;
        private readonly IOrderContextService _tableSession;
        private readonly OrderStore _orderStore;
        private readonly SettingsService _settingsService;

        public TableStore(
            ITableService tableService,
        IOrderContextService tableSession,
        OrderStore orderStore,
        SettingsService settingsService)
        {
            _tableService = tableService;
            _tableSession = tableSession;
            _orderStore = orderStore;
            _settingsService = settingsService;

            Tables = new ObservableCollection<TableViewModel>();
        }

        public async Task LoadAsync()
        {
            Tables.Clear();

            var tables = await _tableService.GetAllAsync();

            foreach (var table in tables)
            {
                Tables.Add(
                    new TableViewModel(
                        table.Number,
                        _tableSession,
                        _orderStore,
                        _settingsService));
            }
        }
    }

}
