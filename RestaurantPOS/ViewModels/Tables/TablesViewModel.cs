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
        public ObservableCollection<TableItemViewModel> Tables { get; }

        public TablesViewModel(INavigationService navigationService)
        {
            Tables = new ObservableCollection<TableItemViewModel>
            {
                new TableItemViewModel(1, false, navigationService),
                new TableItemViewModel(2, true, navigationService),
                new TableItemViewModel(3, false, navigationService),
                new TableItemViewModel(4, false, navigationService),
                new TableItemViewModel(5, true, navigationService),
            };
        }
    }
}
