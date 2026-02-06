using CommunityToolkit.Mvvm.Input;
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
namespace RestaurantPOS.ViewModels.Tables
{
    public class TablesViewModel : ViewModelBase
    {
        public ObservableCollection<TableViewModel> Tables => _tableStore.Tables;

        private readonly TableStore _tableStore;
        public ICommand SelectTableCommand { get; }

        private readonly ITableSessionService _tableSession;
        private readonly INavigationService _navigation;

        public TablesViewModel(
         TableStore tableStore,
         ITableSessionService tableSession,
         INavigationService navigation)
        {
            _tableStore = tableStore;
            _ = LoadTables();
            _tableSession = tableSession;
            _navigation = navigation;

            SelectTableCommand = new RelayCommand<TableViewModel>(OnSelectTable);
        }

        private async Task LoadTables()
        {
            await _tableStore.LoadAsync();
        }
        private void OnSelectTable(TableViewModel table)
        {
            _tableSession.SwitchTable(table.Number);

            _navigation.NavigateTo<OrderViewModel>();
        }
    }

}
