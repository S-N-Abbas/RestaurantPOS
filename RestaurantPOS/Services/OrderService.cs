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

        public OrderService(PosDbContext db)
        {
            _db = db;
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

            var remaining = order.TotalAmount - order.PaidAmount;

            if (method == "Card" && amount != remaining)
                throw new InvalidOperationException("Card must pay exact amount");

            var applied = Math.Min(amount, remaining);

            order.Payments.Add(new Payment
            {
                Amount = applied,
                Method = method,
                PaidAt = DateTime.UtcNow
            });

            RecalculateTotals(order);
            await _db.SaveChangesAsync();
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

            RecalculateTotals(order);
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
            MenuItemViewModel item,
            int quantityChange)
        {
            var order = await LoadOrderAsync(orderId);

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

            RecalculateTotals(order);
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

            RecalculateTotals(order);
            await _db.SaveChangesAsync();
        }


        public async Task AddItemAsync(
    Order order,
    MenuItemViewModel item)
        {
            using var db = _db;

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

            RecalculateTotals(order);

            await db.SaveChangesAsync();
        }

        public async Task<Order> GetByIdAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstAsync(o => o.Id == orderId);
        }


        private static void RecalculateTotals(Order dbOrder)
        {
            dbOrder.TotalAmount = dbOrder.Items.Sum(i => i.UnitPrice * i.Quantity);
            dbOrder.PaidAmount = dbOrder.Payments.Sum(p => p.Amount);
        }

        public async Task CloseOrderAsync(int orderId)
        {
            var order = await LoadOrderAsync(orderId);

            if (order.IsClosed)
                return;

            if (order.PaidAmount < order.TotalAmount)
                throw new InvalidOperationException("Order not fully paid");

            order.IsClosed = true;
            order.ClosedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
