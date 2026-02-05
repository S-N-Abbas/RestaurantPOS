using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Orders
{
    public class CategoryViewModel : ViewModelBase
    {
        public int Id { get; }
        public string Name { get; }

        public CategoryViewModel(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
