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

        public TableStore(
            ITableSessionService tableSession,
            OrderStore orderStore)
        {
            Tables = new ObservableCollection<TableViewModel>();

            for (int i = 1; i <= 12; i++)
            {
                Tables.Add(new TableViewModel(i, tableSession, orderStore));
            }
        }
    }

}
