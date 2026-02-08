using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Domain.Entities
{
    public enum PaymentMethod
    {
        Cash = 1,
        Card = 2
    }

    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string Method { get; set; } = ""; // Cash / Card

        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; }
    }

}
