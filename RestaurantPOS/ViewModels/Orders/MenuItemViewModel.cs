using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Orders
{
    public class MenuItemViewModel : ViewModelBase
    {
        public int Id { get; }
        public string Name { get; }
        public decimal Price { get; }
        public int CategoryId { get; }

        public MenuItemViewModel(int id, string name, decimal price, int categoryId)
        {
            Id = id;
            Name = name;
            Price = price;
            CategoryId = categoryId;
        }
    }
}
