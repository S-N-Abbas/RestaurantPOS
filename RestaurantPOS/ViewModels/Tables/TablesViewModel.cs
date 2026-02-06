using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantPOS.ViewModels.Tables
{
    public class TablesViewModel : ViewModelBase
    {
        public ObservableCollection<TableViewModel> Tables { get; }

        public TablesViewModel(TableStore tableStore)
        {
            Tables = tableStore.Tables;
        }
    }

}
