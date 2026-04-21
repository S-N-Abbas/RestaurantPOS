using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Repositories
{
    /// <summary>
    /// Aggregated figures for one section of the Z-Report.
    /// Used for Overall, per-Till, per-User, and per-OrderType breakdowns.
    /// </summary>
    public class ZReportSection
    {
        public string Label { get; init; } = string.Empty;  // "Overall" / "Till 01" / "Alice"
        public int OrderCount { get; init; }
        public int CancelCount { get; init; }
        public decimal CashTotal { get; init; }
        public decimal CardTotal { get; init; }
        public decimal DepositTotal { get; init; }
        public decimal GrandTotal => CashTotal + CardTotal + DepositTotal;
        public decimal AverageOrder => OrderCount > 0 ? GrandTotal / OrderCount : 0m;
        public int AdultCovers { get; init; }
        public int ChildCovers { get; init; }
    }

    /// <summary>
    /// Full Z-Report data bundle — passed to ZReportBuilder.
    /// </summary>
    public class ZReportData
    {
        public DateTime From { get; init; }
        public DateTime To { get; init; }
        public string GeneratedBy { get; init; } = string.Empty;
        public string TillNo { get; init; } = string.Empty;

        public ZReportSection Overall { get; init; } = new();

        /// <summary>Breakdown by OrderType (DineIn, TakeAway, Delivery).</summary>
        public IReadOnlyList<ZReportSection> ByOrderType { get; init; } = [];

        /// <summary>Breakdown per till number.</summary>
        public IReadOnlyList<ZReportSection> ByTill { get; init; } = [];

        /// <summary>Breakdown per staff member.</summary>
        public IReadOnlyList<ZReportSection> ByUser { get; init; } = [];
    }
}
