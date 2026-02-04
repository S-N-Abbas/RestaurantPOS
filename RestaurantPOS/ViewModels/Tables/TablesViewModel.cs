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
        public ObservableCollection<TableItemViewModel> Tables { get; }

        public TablesViewModel()
        {
            Tables = new ObservableCollection<TableItemViewModel>
            {
                new TableItemViewModel(1, false),
                new TableItemViewModel(2, true),
                new TableItemViewModel(3, false),
                new TableItemViewModel(4, false),
                new TableItemViewModel(5, true),
            };
        }
    }
}
