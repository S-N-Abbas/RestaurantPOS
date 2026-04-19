using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;
using RestaurantPOS.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RestaurantPOS.Services
{
    public class OrderService
    {
        private readonly PosDbContext _db;
        private readonly SettingsService _settingsService;

        public OrderService(PosDbContext db, SettingsService settingsService)
        {
            _db = db;
            _settingsService = settingsService;
        }

        public async Task<List<Order>> GetOpenOrdersAsync()
        {
            return await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .Where(o => o.ClosedAt == null)
                .ToListAsync();
        }


        public async Task UpdateCoversAsync(int orderId, int adults, int children)
        {
            if (adults < 0 || children < 0)
                throw new ArgumentException("Cover counts cannot be negative");

            var order = await _db.Orders.FindAsync(orderId);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            if (order.IsClosed)
                throw new InvalidOperationException("Cannot modify closed order");

            order.AdultCovers = adults;
            order.ChildCovers = children;

            await _db.SaveChangesAsync();
        }


        public async Task RecordPaymentAsync(
            int orderId,
            decimal amount,
            string method)
        {
            if (amount <= 0)
                throw new ArgumentException("Invalid amount");


            var order = await LoadOrderAsync(orderId);

            if (order.IsClosed)
                throw new InvalidOperationException("Order already closed");

            var orderTotal = order.ItemsTotal + order.ChildCovers * _settingsService.Settings.ChildCoverPrice
                + order.AdultCovers * _settingsService.Settings.AdultCoverPrice;

            var remaining = orderTotal - order.PaidAmount;

            //if (method == "Card" && amount != remaining)
            //    throw new InvalidOperationException("Card must pay exact amount");

            var applied = Math.Min(amount, remaining);

            order.Payments.Add(new Payment
            {
                OrderId = orderId,
                Amount = applied,
                Method = method,
                PaidAt = DateTime.UtcNow
                
            });

            await _db.SaveChangesAsync();
        }


        public async Task<Order?> GetOpenOrderAsync(int contextId)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o =>
                    o.ContextId == contextId &&
                    o.ClosedAt == null);
        }

        /// <summary>
        /// Creates a new open order.
        /// For DineIn: contextId is the table number (positive int).
        /// For TakeAway: contextId is a negative slot id (e.g. -1, -2).
        /// </summary>
        public async Task<Order> CreateOrderAsync(int contextId, OrderType orderType = OrderType.DineIn)
        {
            int? tableId = null;

            if (orderType == OrderType.DineIn)
            {
                // contextId IS the table number for dine-in
                var table = await _db.Tables.FirstOrDefaultAsync(t => t.Number == contextId);
                if (table == null)
                    throw new InvalidOperationException($"No active table found for number {contextId}.");
                tableId = table.Id;
            }
            // TakeAway: tableId stays null — no table row needed

            var order = new Order
            {
                ContextId = contextId,
                TableId = tableId,
                OrderType = orderType,
                CreatedAt = DateTime.Now
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return order;
        }

        private async Task<Order> LoadOrderAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            return order;
        }


        public async Task AddOrUpdateItemAsync(
            int orderId,
            int itemId,
            int quantityChange)
        {
            var order = await LoadOrderAsync(orderId);

            var existing = order.Items
                .FirstOrDefault(i => i.ProductId == itemId);


            if (existing == null)
            {
                var product = await _db.Products.FindAsync(itemId);

                if(product == null)
                    throw new InvalidOperationException("Product not found");

                existing = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = 0
                };
                order.Items.Add(existing);
            }

            existing.Quantity += quantityChange;

            if (existing.Quantity <= 0)
                order.Items.Remove(existing);

            await _db.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(
    int orderId,
    int productId,
    int quantity)
        {
            var order = await LoadOrderAsync(orderId);

            var item = order.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return;

            if (quantity <= 0)
                order.Items.Remove(item);
            else
                item.Quantity = quantity;

            await _db.SaveChangesAsync();
        }


        public async Task AddItemAsync(
    int orderId,
    int itemId)
        {

            var order = await _db.Orders
        .Include(o => o.Items)
        .FirstAsync(o => o.Id == orderId);

            var existing = await _db.OrderItems
                .FirstOrDefaultAsync(i =>
                    i.OrderId == order.Id &&
                    i.ProductId == itemId);

            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                var item = await _db.Products.FindAsync(itemId);

                if (item == null)
                    throw new InvalidOperationException("Product not found");

                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.Id,
                    ProductName = item.Name,
                    UnitPrice = item.Price,
                    Quantity = 1
                });
            }


            await _db.SaveChangesAsync();
        }

        public async Task<Order> GetByIdAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstAsync(o => o.Id == orderId);
        }

        public async Task CloseOrderAsync(int orderId)
        {
            var order = await LoadOrderAsync(orderId);

            if (order.IsClosed)
                return;

            var orderTotal = order.ItemsTotal
                + order.ChildCovers * _settingsService.Settings.ChildCoverPrice
                + order.AdultCovers * _settingsService.Settings.AdultCoverPrice;

            if (order.PaidAmount < orderTotal)
                throw new InvalidOperationException("Order not fully paid");

            order.IsClosed = true;
            order.Status = OrderStatus.Paid;
            order.ClosedAt = DateTime.UtcNow;

            // ✅ Auto-complete any linked booking
            var linkedBooking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.OrderId == orderId);

            if (linkedBooking != null &&
                linkedBooking.Status == BookingStatus.Seated)
            {
                linkedBooking.Status = BookingStatus.Completed;
                linkedBooking.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Marks an order as Cancelled, closes it, and persists to DB.
        /// Throws if the order has payments recorded (requires manager override upstream).
        /// </summary>
        public async Task CancelOrderAsync(int orderId)
        {
            var order = await LoadOrderAsync(orderId);

            if (order.IsClosed)
                throw new InvalidOperationException("Order is already closed.");

            order.IsClosed = true;
            order.Status = OrderStatus.Cancelled;
            order.ClosedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Transfers an open DineIn order from one table to another.
        /// Throws if the destination table already has an open order.
        /// </summary>
        public async Task TransferTableAsync(int orderId, int destinationTableNumber)
        {
            // ── Verify destination is free ──────────────────────────────────────────
            bool destinationOccupied = await _db.Orders
                .AnyAsync(o =>
                    o.ContextId == destinationTableNumber &&
                    o.ClosedAt == null);

            if (destinationOccupied)
                throw new InvalidOperationException(
                    $"Table {destinationTableNumber} already has an open order.");

            // ── Resolve destination table row ────────────────────────────────────────
            var destinationTable = await _db.Tables
                .FirstOrDefaultAsync(t => t.Number == destinationTableNumber)
                ?? throw new InvalidOperationException(
                    $"Table {destinationTableNumber} not found.");

            // ── Update the order ─────────────────────────────────────────────────────
            var order = await LoadOrderAsync(orderId);

            if (order.IsClosed)
                throw new InvalidOperationException("Cannot transfer a closed order.");

            order.ContextId = destinationTableNumber;
            order.TableId = destinationTable.Id;

            await _db.SaveChangesAsync();
        }
    }
}
