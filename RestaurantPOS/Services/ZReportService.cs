using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;
using RestaurantPOS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RestaurantPOS.Services
{
    public class ZReportService : IZReportService
    {
        private readonly PosDbContext _db;
        private readonly UserSessionService _userSessionService;
        private readonly SettingsService _settingsService;

        public ZReportService(
            PosDbContext db,
            UserSessionService userSessionService,
            SettingsService settingsService)
        {
            _db = db;
            _userSessionService = userSessionService;
            _settingsService = settingsService;
        }

        public async Task<ZReportData> GenerateAsync(DateTime from, DateTime to)
        {
            // ── Load all closed orders in range ───────────────────────────────
            var orders = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .Where(o =>
                    o.IsClosed &&
                    o.ClosedAt >= from &&
                    o.ClosedAt <= to)
                .AsNoTracking()
                .ToListAsync();

            var cancelled = await _db.Orders
                .Where(o =>
                    o.Status == OrderStatus.Cancelled &&
                    o.ClosedAt >= from &&
                    o.ClosedAt <= to)
                .CountAsync();

            return new ZReportData
            {
                From = from,
                To = to,
                GeneratedBy = _userSessionService.CurrentUser?.Username ?? "Unknown",
                TillNo = _settingsService.Settings.TillNo,
                Overall = BuildSection("Overall", orders, cancelled),
                ByOrderType = BuildByOrderType(orders),
                ByTill = BuildByTill(orders),
                ByUser = BuildByUser(orders)
            };
        }

        // ─── Section Builders ─────────────────────────────────────────────────

        private ZReportSection BuildSection(
    string label,
    IEnumerable<Order> orders,
    int cancelCount = 0)
        {
            var list = orders.ToList();

            decimal cashTotal = 0m;
            decimal cardTotal = 0m;
            decimal depositTotal = 0m;

            foreach (var order in list)
            {
                // ── Actual grand total for this order ─────────────────────────────────
                decimal coverTotal = _settingsService.CalculateCoverCharge(order);

                decimal orderGrandTotal = order.ItemsTotal + coverTotal;

                // ── Raw payment totals ────────────────────────────────────────────────
                decimal rawCash = order.Payments
                    .Where(p => p.Method == "Cash")
                    .Sum(p => p.Amount);

                decimal rawCard = order.Payments
                    .Where(p => p.Method == "Card")
                    .Sum(p => p.Amount);

                decimal rawDeposit = order.Payments
                    .Where(p => p.Method == "Deposit")
                    .Sum(p => p.Amount);

                // ── Card and deposit are always exact — never overpay ─────────────────
                cardTotal += rawCard;
                depositTotal += rawDeposit;

                // ── Cash net revenue = grand total minus what card and deposit covered ─
                // This strips out any change given back to the customer
                decimal coveredByOther = rawCard + rawDeposit;
                decimal cashRevenue = Math.Max(orderGrandTotal - coveredByOther, 0m);

                // ✅ Cap at actual cash tendered in case order was partially paid
                cashTotal += Math.Min(cashRevenue, rawCash);
            }

            return new ZReportSection
            {
                Label = label,
                OrderCount = list.Count,
                CancelCount = cancelCount,
                CashTotal = cashTotal,
                CardTotal = cardTotal,
                DepositTotal = depositTotal,
                AdultCovers = list.Sum(o => o.AdultCovers),
                ChildCovers = list.Sum(o => o.ChildCovers)
            };
        }

        private IReadOnlyList<ZReportSection> BuildByOrderType(List<Order> orders)
        {
            return new[]
            {
                OrderType.DineIn,
                OrderType.TakeAway,
                OrderType.Delivery
            }
            .Select(type => BuildSection(
                type.ToString(),
                orders.Where(o => o.OrderType == type)))
            .ToList();
        }

        private IReadOnlyList<ZReportSection> BuildByTill(List<Order> orders)
        {
            return orders
                .GroupBy(o => string.IsNullOrWhiteSpace(o.TillNo) ? "Unknown" : o.TillNo)
                .OrderBy(g => g.Key)
                .Select(g => BuildSection($"Till {g.Key}", g))
                .ToList();
        }

        private IReadOnlyList<ZReportSection> BuildByUser(List<Order> orders)
        {
            return orders
                .GroupBy(o => string.IsNullOrWhiteSpace(o.ClosedBy) ? "Unknown" : o.ClosedBy)
                .OrderBy(g => g.Key)
                .Select(g => BuildSection(g.Key, g))
                .ToList();
        }
    }
}
