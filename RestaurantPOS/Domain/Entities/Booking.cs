using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantPOS.Domain.Entities
{

    public enum BookingStatus
    {
        Pending = 1,  // Created, not yet confirmed
        Confirmed = 2,  // Staff confirmed via phone/email
        Seated = 3,  // Customer arrived, live order created
        Completed = 4,  // Order paid and closed
        Cancelled = 5,  // Cancelled by customer or staff
        NoShow = 6   // Customer did not arrive
    }

    public class Booking
    {
        public int Id { get; set; }

        // ─── Customer Details ─────────────────────────────────────────────────
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // ─── Bookings Details ──────────────────────────────────────────────────
        public int PartySize { get; set; }
        public DateTime BookingDate { get; set; }    // Date + Time combined
        public string Notes { get; set; } = string.Empty;

        // ─── Table Assignment (optional until seating) ────────────────────────
        public int? TableId { get; set; }
        public Table? Table { get; set; }

        // ─── Status ───────────────────────────────────────────────────────────
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // ─── Deposit ──────────────────────────────────────────────────────────
        public decimal DepositAmount { get; set; } = 0m;
        public bool DepositPaid { get; set; } = false;
        public string DepositMethod { get; set; } = string.Empty; // Cash / Card
        public DateTime? DepositPaidAt { get; set; }

        // ─── Linked Order (set when customer is seated) ───────────────────────
        public int? OrderId { get; set; }
        public Order? Order { get; set; }

        // ─── Audit ────────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ─── Computed (not mapped) ────────────────────────────────────────────

        [NotMapped]
        public bool IsUpcoming =>
            Status == BookingStatus.Confirmed ||
            Status == BookingStatus.Pending;

        [NotMapped]
        public bool CanBeSat =>
            Status == BookingStatus.Confirmed ||
            Status == BookingStatus.Pending;

        [NotMapped]
        public bool HasDeposit => DepositPaid && DepositAmount > 0;

        [NotMapped]
        public TimeSpan TimeUntil => BookingDate - DateTime.Now;
    }
}
