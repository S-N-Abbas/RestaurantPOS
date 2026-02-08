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

namespace RestaurantPOS.Services
{
    public class OrderService
    {
        private readonly PosDbContext _db;
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public OrderService(PosDbContext db, IDbContextFactory<PosDbContext> contextFactory)
        {
            _db = db;
            _contextFactory = contextFactory;
        }

        public async Task RecordPaymentAsync(
        Order order,
        decimal amount,
        string method)
        {
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero");

            using var context = _contextFactory.CreateDbContext();
            using var tx = await context.Database.BeginTransactionAsync();

            // Reload order safely
            var dbOrder = await context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (dbOrder == null)
                throw new InvalidOperationException("Order not found");

            if (dbOrder.IsClosed)
                throw new InvalidOperationException("Order already closed");

            var remaining = dbOrder.TotalAmount - dbOrder.PaidAmount;

            // Card must not overpay
            if (method == "Card" && amount != remaining)
                throw new InvalidOperationException("Card payment must equal remaining amount");

            // Cap cash overpayment (change handled in UI)
            var appliedAmount = Math.Min(amount, remaining);

            var payment = new Payment
            {
                OrderId = dbOrder.Id,
                Amount = appliedAmount,
                Method = method,
                PaidAt = DateTime.UtcNow
            };

            context.Payments.Add(payment);

            dbOrder.PaidAmount += appliedAmount;

            await context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<Order?> GetOpenOrderAsync(int tableNumber)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o =>
                    o.TableNumber == tableNumber &&
                    o.ClosedAt == null);
        }

        public async Task<Order> CreateOrderAsync(int tableNumber)
        {
            var table = await _db.Tables.FirstAsync(t => t.Number == tableNumber);

            var order = new Order
            {
                TableNumber = tableNumber,
                TableId = table.Id,
                CreatedAt = DateTime.Now
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return order;
        }

        public async Task AddOrUpdateItemAsync(
            Order order,
            MenuItemViewModel item,
            int quantityChange)
        {
            var existing = order.Items
                .FirstOrDefault(i => i.ProductId == item.Id);

            if (existing == null)
            {
                existing = new OrderItem
                {
                    ProductId = item.Id,
                    ProductName = item.Name,
                    UnitPrice = item.Price,
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
    Order order,
    int menuItemId,
    int quantity)
        {
            using var db = _contextFactory.CreateDbContext();

            var item = await db.OrderItems
                .FirstOrDefaultAsync(i =>
                    i.OrderId == order.Id &&
                    i.ProductId == menuItemId);

            if (item == null)
                return;

            if (quantity <= 0)
            {
                db.OrderItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            await db.SaveChangesAsync();
        }

        public async Task AddItemAsync(
    Order order,
    MenuItemViewModel item)
        {
            using var db = _contextFactory.CreateDbContext();

            var existing = await db.OrderItems
                .FirstOrDefaultAsync(i =>
                    i.OrderId == order.Id &&
                    i.ProductId == item.Id);

            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.Id,
                    ProductName = item.Name,
                    UnitPrice = item.Price,
                    Quantity = 1
                });
            }

            await db.SaveChangesAsync();
        }


        public async Task CloseOrderAsync(Order order)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbOrder = await context.Orders.FindAsync(order.Id);
            if (dbOrder == null)
                return;

            if (dbOrder.IsClosed)
                return;

            if (dbOrder.PaidAmount < dbOrder.TotalAmount)
                throw new InvalidOperationException("Order not fully paid");

            dbOrder.IsClosed = true;
            dbOrder.ClosedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }
    }
}
