using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface IBookingService
    {
        // ─── Queries ──────────────────────────────────────────────────────────

        /// <summary>All bookings for today, ordered by time.</summary>
        Task<IReadOnlyList<Booking>> GetTodaysBookingsAsync();

        /// <summary>All bookings for a specific date, ordered by time.</summary>
        Task<IReadOnlyList<Booking>> GetBookingsByDateAsync(DateOnly date);

        /// <summary>
        /// Upcoming confirmed/pending bookings from now onwards,
        /// across all dates. Used for the home screen badge count.
        /// </summary>
        Task<IReadOnlyList<Booking>> GetUpcomingBookingsAsync();

        /// <summary>Single booking with all navigation properties loaded.</summary>
        Task<Booking> GetByIdAsync(int id);

        // ─── Create / Update ──────────────────────────────────────────────────

        /// <summary>Creates a new booking. Returns the saved entity.</summary>
        Task<Booking> CreateBookingAsync(
            string customerName,
            string customerPhone,
            string customerEmail,
            int partySize,
            DateTime bookingDate,
            int? tableId,
            string notes,
            decimal depositAmount,
            bool depositPaid,
            string depositMethod);

        /// <summary>Updates an existing booking's details.</summary>
        Task<Booking> UpdateBookingAsync(
            int id,
            string customerName,
            string customerPhone,
            string customerEmail,
            int partySize,
            DateTime bookingDate,
            int? tableId,
            string notes);

        /// <summary>
        /// Records a deposit payment against an existing booking.
        /// Can be called separately if deposit is taken after initial creation.
        /// </summary>
        Task RecordDepositAsync(int bookingId, decimal amount, string method);

        // ─── Status Transitions ───────────────────────────────────────────────

        /// <summary>
        /// Moves a Pending booking to Confirmed.
        /// Typically called after a phone confirmation with the customer.
        /// </summary>
        Task ConfirmBookingAsync(int bookingId);

        /// <summary>
        /// Seats the customer — creates a live Order, pre-records the deposit
        /// as a payment, links the order to the booking, and sets status to Seated.
        /// Returns the newly created Order so the caller can navigate to OrderView.
        /// </summary>
        Task<Order> SeatBookingAsync(int bookingId, int tableNumber);

        /// <summary>
        /// Cancels a booking. If the deposit was paid, the caller decides
        /// whether to refund it — this is recorded in the notes.
        /// </summary>
        Task CancelBookingAsync(int bookingId, bool refundDeposit, string? reason = null);

        /// <summary>Marks a booking as a no-show. Deposit is forfeited.</summary>
        Task MarkNoShowAsync(int bookingId);

        /// <summary>
        /// Marks a booking as Completed. Called automatically when the
        /// linked order is paid and closed.
        /// </summary>
        Task CompleteBookingAsync(int bookingId);
    }
}
