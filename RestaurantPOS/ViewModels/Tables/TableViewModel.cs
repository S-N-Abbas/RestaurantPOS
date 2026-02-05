using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Tables
{
    public class TableViewModel : ViewModelBase
    {
        public int Number { get; }

        private bool _hasOrder;
        public bool HasOrder
        {
            get => _hasOrder;
            set => SetProperty(ref _hasOrder, value);
        }

        public TableViewModel(int number)
        {
            Number = number;
        }
    }
}
