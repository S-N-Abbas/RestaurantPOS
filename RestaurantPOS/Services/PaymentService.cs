using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public PaymentService(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddPaymentAsync(
            Order order,
            PaymentMethod method,
            decimal amount)
        {
            using var ctx = _contextFactory.CreateDbContext();

            string methodStr = method switch
            {
                PaymentMethod.Cash => "Cash",
                PaymentMethod.Card => "Card",
                _ => throw new ArgumentOutOfRangeException(nameof(method), "Unsupported payment method")
            };

            ctx.Payments.Add(new Payment
            {
                OrderId = order.Id,
                Method = methodStr,
                Amount = amount,
                PaidAt = DateTime.UtcNow
            });

            await ctx.SaveChangesAsync();
        }

        public async Task<bool> IsOrderFullyPaidAsync(Order order)
        {
            using var ctx = _contextFactory.CreateDbContext();

            var totalPaid = await ctx.Payments
                .Where(p => p.OrderId == order.Id)
                .SumAsync(p => p.Amount);

            return totalPaid >= order.Items.Sum(i => i.UnitPrice * i.Quantity);
        }

        public async Task CloseIfPaidAsync(Order order)
        {
            if (!await IsOrderFullyPaidAsync(order))
                return;

            using var ctx = _contextFactory.CreateDbContext();

            var dbOrder = await ctx.Orders.FindAsync(order.Id);
            if (dbOrder == null) return;

            dbOrder.Status = OrderStatus.Paid;
            await ctx.SaveChangesAsync();
        }
    }

}
