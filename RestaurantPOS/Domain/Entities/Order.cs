using RestaurantPOS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

    public enum OrderType
    {
        DineIn = 1,
        TakeAway = 2,
        Delivery = 3
    }

    public class Order
    {
        public int Id { get; set; }

        public int ContextId { get; set; }
        public int? TableId { get; set; }

        public Table? Table { get; set; } = null!;

        //Distinguishes order type explicitly
        public OrderType OrderType { get; set; } = OrderType.DineIn;

        // ✅ Covers (Persisted)
        public int AdultCovers { get; set; }
        public int ChildCovers { get; set; }

        public string? CoverALabel { get; set; }   // replaces "Adults"
        public decimal? CoverAPrice { get; set; }  // per-unit override
        public string? CoverBLabel { get; set; }   // replaces "Children"
        public decimal? CoverBPrice { get; set; }  // per-unit override

        public string TillNo { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public string ClosedBy { get; set; } = string.Empty;

        [NotMapped]
        public decimal ItemsTotal => Items?.Sum(i => i.UnitPrice * i.Quantity) ?? 0m;

        [NotMapped]
        public decimal PaidAmount => Payments?.Sum(p => p.Amount) ?? 0m;
        public bool IsClosed { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Open;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
