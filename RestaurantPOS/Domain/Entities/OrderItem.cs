using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantPOS.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int? ProductId { get; set; }      // ✅ nullable — null for open items
        public MenuProduct? Product { get; set; } // ✅ nullable navigation property

        public string ProductName { get; set; } = null!;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        [NotMapped]
        public decimal LineTotal => UnitPrice * Quantity;
        
        [NotMapped]
        public bool IsOpenItem => ProductId == null;

    }

}
