using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Domain.Entities
{
    public enum OrderStatus
    {
        Open = 1,
        Paid = 2,
        Cancelled = 3
    }

    public class Order
    {
        public int Id { get; set; }

        public int TableNumber { get; set; }
        public int TableId { get; set; }

        public Table Table { get; set; } = null!;

        public decimal PaidAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsClosed { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Open;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
