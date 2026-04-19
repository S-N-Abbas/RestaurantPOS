using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;

namespace RestaurantPOS.Services
{
    public class BookingService : IBookingService
    {
        private readonly PosDbContext _db;

        public BookingService(PosDbContext db)
        {
            _db = db;
        }

        // ─── Queries ──────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<Booking>> GetTodaysBookingsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _db.Bookings
                .Include(b => b.Table)
                .Where(b =>
                    b.BookingDate >= today &&
                    b.BookingDate < tomorrow)
                .OrderBy(b => b.BookingDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Booking>> GetBookingsByDateAsync(DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = date.ToDateTime(TimeOnly.MaxValue);

            return await _db.Bookings
                .Include(b => b.Table)
                .Where(b => b.BookingDate >= start && b.BookingDate <= end)
                .OrderBy(b => b.BookingDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Booking>> GetUpcomingBookingsAsync()
        {
            return await _db.Bookings
                .Include(b => b.Table)
                .Where(b =>
                    (b.Status == BookingStatus.Pending ||
                     b.Status == BookingStatus.Confirmed) &&
                    b.BookingDate >= DateTime.Now)
                .OrderBy(b => b.BookingDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Booking> GetByIdAsync(int id)
        {
            return await _db.Bookings
                .Include(b => b.Table)
                .Include(b => b.Order)
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new InvalidOperationException($"Booking #{id} not found.");
        }

        // ─── Create ───────────────────────────────────────────────────────────

        public async Task<Booking> CreateBookingAsync(
            string customerName,
            string customerPhone,
            string customerEmail,
            int partySize,
            DateTime bookingDate,
            int? tableId,
            string notes,
            decimal depositAmount,
            bool depositPaid,
            string depositMethod)
        {
            ValidateCreateInputs(customerName, partySize, bookingDate, depositAmount);

            // If a table is being pre-assigned, check it isn't already booked
            // for the same time slot (within a 2-hour window)
            if (tableId.HasValue)
                await GuardTableAvailabilityAsync(tableId.Value, bookingDate, excludeBookingId: null);

            var booking = new Booking
            {
                CustomerName = customerName.Trim(),
                CustomerPhone = customerPhone.Trim(),
                CustomerEmail = customerEmail.Trim(),
                PartySize = partySize,
                BookingDate = bookingDate,
                TableId = tableId,
                Notes = notes.Trim(),
                DepositAmount = depositAmount,
                DepositPaid = depositPaid,
                DepositMethod = depositPaid ? depositMethod : string.Empty,
                DepositPaidAt = depositPaid ? DateTime.UtcNow : null,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return booking;
        }

        // ─── Update ───────────────────────────────────────────────────────────

        public async Task<Booking> UpdateBookingAsync(
            int id,
            string customerName,
            string customerPhone,
            string customerEmail,
            int partySize,
            DateTime bookingDate,
            int? tableId,
            string notes)
        {
            var booking = await LoadBookingAsync(id);

            GuardNotClosed(booking);
            ValidateCreateInputs(customerName, partySize, bookingDate, depositAmount: 0);

            if (tableId.HasValue)
                await GuardTableAvailabilityAsync(tableId.Value, bookingDate, excludeBookingId: id);

            booking.CustomerName = customerName.Trim();
            booking.CustomerPhone = customerPhone.Trim();
            booking.CustomerEmail = customerEmail.Trim();
            booking.PartySize = partySize;
            booking.BookingDate = bookingDate;
            booking.TableId = tableId;
            booking.Notes = notes.Trim();
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return booking;
        }

        public async Task RecordDepositAsync(int bookingId, decimal amount, string method)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentException("Payment method is required.");

            var booking = await LoadBookingAsync(bookingId);

            GuardNotClosed(booking);

            if (booking.DepositPaid)
                throw new InvalidOperationException("A deposit has already been recorded for this booking.");

            booking.DepositAmount = amount;
            booking.DepositPaid = true;
            booking.DepositMethod = method;
            booking.DepositPaidAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─── Status Transitions ───────────────────────────────────────────────

        public async Task ConfirmBookingAsync(int bookingId)
        {
            var booking = await LoadBookingAsync(bookingId);

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException(
                    $"Only Pending bookings can be confirmed. Current status: {booking.Status}.");

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task<Order> SeatBookingAsync(int bookingId, int tableNumber)
        {
            var booking = await LoadBookingAsync(bookingId);

            // ── Guards ────────────────────────────────────────────────────────
            if (!booking.CanBeSat)
                throw new InvalidOperationException(
                    $"Booking cannot be seated. Current status: {booking.Status}.");

            // Verify the destination table exists
            var table = await _db.Tables
                .FirstOrDefaultAsync(t => t.Number == tableNumber)
                ?? throw new InvalidOperationException(
                    $"Table {tableNumber} not found.");

            // Verify no open order already exists on that table
            bool tableOccupied = await _db.Orders
                .AnyAsync(o =>
                    o.ContextId == tableNumber &&
                    o.ClosedAt == null);

            if (tableOccupied)
                throw new InvalidOperationException(
                    $"Table {tableNumber} already has an open order. " +
                    "Please choose a different table or close the existing order first.");

            // ── Create the live Order ─────────────────────────────────────────
            var order = new Order
            {
                ContextId = tableNumber,
                TableId = table.Id,
                OrderType = OrderType.DineIn,
                AdultCovers = booking.PartySize,  // pre-fill from booking
                ChildCovers = 0,
                CreatedAt = DateTime.Now,
                Status = OrderStatus.Open
            };

            _db.Orders.Add(order);

            // ── Pre-record the deposit as a payment ───────────────────────────
            if (booking.HasDeposit)
            {
                order.Payments.Add(new Payment
                {
                    Amount = booking.DepositAmount,
                    Method = "Deposit",
                    PaidAt = DateTime.UtcNow
                });
            }

            // ── Link booking to order and update status ───────────────────────
            booking.Status = BookingStatus.Seated;
            booking.UpdatedAt = DateTime.UtcNow;

            // We need to save first to get the Order.Id, then link
            await _db.SaveChangesAsync();

            booking.OrderId = order.Id;
            await _db.SaveChangesAsync();

            // ── Return fully loaded order ─────────────────────────────────────
            return await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstAsync(o => o.Id == order.Id);
        }

        public async Task CancelBookingAsync(int bookingId, bool refundDeposit, string? reason = null)
        {
            var booking = await LoadBookingAsync(bookingId);

            GuardNotClosed(booking);

            var note = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(reason))
                note.Append($"Cancellation reason: {reason}. ");

            if (booking.DepositPaid)
            {
                note.Append(refundDeposit
                    ? $"Deposit of £{booking.DepositAmount:N2} refunded."
                    : $"Deposit of £{booking.DepositAmount:N2} forfeited.");

                // If refunded, zero the deposit so it doesn't show
                // as pre-payment when reviewing history
                if (refundDeposit)
                {
                    booking.DepositPaid = false;
                    booking.DepositAmount = 0m;
                }
            }

            // Append note to existing notes so history is preserved
            if (note.Length > 0)
            {
                booking.Notes = string.IsNullOrWhiteSpace(booking.Notes)
                    ? note.ToString()
                    : $"{booking.Notes} | {note}";
            }

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task MarkNoShowAsync(int bookingId)
        {
            var booking = await LoadBookingAsync(bookingId);

            if (booking.Status != BookingStatus.Confirmed &&
                booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException(
                    $"Cannot mark as no-show. Current status: {booking.Status}.");

            // Deposit is forfeited — no changes to deposit fields needed,
            // the status change alone records the outcome
            booking.Status = BookingStatus.NoShow;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task CompleteBookingAsync(int bookingId)
        {
            var booking = await LoadBookingAsync(bookingId);

            if (booking.Status != BookingStatus.Seated)
                throw new InvalidOperationException(
                    $"Only Seated bookings can be completed. Current status: {booking.Status}.");

            booking.Status = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ─── Private Helpers ──────────────────────────────────────────────────

        private async Task<Booking> LoadBookingAsync(int id)
        {
            return await _db.Bookings
                .Include(b => b.Table)
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new InvalidOperationException($"Booking #{id} not found.");
        }

        private static void GuardNotClosed(Booking booking)
        {
            if (booking.Status == BookingStatus.Cancelled ||
                booking.Status == BookingStatus.Completed ||
                booking.Status == BookingStatus.NoShow)
                throw new InvalidOperationException(
                    $"Cannot modify a booking with status: {booking.Status}.");
        }

        private static void ValidateCreateInputs(
            string customerName,
            int partySize,
            DateTime bookingDate,
            decimal depositAmount)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name is required.");

            if (partySize <= 0)
                throw new ArgumentException("Party size must be at least 1.");

            if (bookingDate <= DateTime.Now)
                throw new ArgumentException("Booking date must be in the future.");

            if (depositAmount < 0)
                throw new ArgumentException("Deposit amount cannot be negative.");
        }

        /// <summary>
        /// Checks whether a table is already booked within a 2-hour window
        /// of the requested time. Excludes the current booking when updating.
        /// </summary>
        private async Task GuardTableAvailabilityAsync(
            int tableId,
            DateTime requestedTime,
            int? excludeBookingId)
        {
            var windowStart = requestedTime.AddHours(-2);
            var windowEnd = requestedTime.AddHours(2);

            var conflict = await _db.Bookings
                .AnyAsync(b =>
                    b.TableId == tableId &&
                    b.BookingDate >= windowStart &&
                    b.BookingDate <= windowEnd &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.NoShow &&
                    b.Status != BookingStatus.Completed &&
                    (excludeBookingId == null || b.Id != excludeBookingId));

            if (conflict)
                throw new InvalidOperationException(
                    "This table is already booked within 2 hours of the requested time. " +
                    "Please choose a different table or time.");
        }
    }
}
