using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<MenuProduct> Products { get; set; } = new List<MenuProduct>();
    }
}
