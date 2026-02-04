using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Orders
{
    public class OrderViewModel : ViewModelBase
    {
        private int _tableNumber;

        public int TableNumber
        {
            get => _tableNumber;
            set => SetProperty(ref _tableNumber, value);
        }
    }
}
