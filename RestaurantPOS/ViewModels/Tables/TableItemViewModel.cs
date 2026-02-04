using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Tables
{
    public class TableItemViewModel : ViewModelBase
    {
        public int TableNumber { get; }
        public bool IsOccupied { get; }

        public TableItemViewModel(int tableNumber, bool isOccupied)
        {
            TableNumber = tableNumber;
            IsOccupied = isOccupied;
        }
    }
}
