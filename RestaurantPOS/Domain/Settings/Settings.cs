using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Domain.Settings
{
    public class AppSettings
    {
        // ___ System Details ---
        public string HostName { get; set; } = string.Empty;

        // --- Business Identity ---
        public string BusinessName { get; set; } = "Nawab Palace";
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;

        // --- Legal & Tax ---
        public string VatNo { get; set; } = string.Empty;
        public double DefaultVatRate { get; set; } = 20.0; // Standard UK VAT
        public bool IsVatEnabled { get; set; } = true;

        // --- Terminal Configuration ---
        public string TillNo { get; set; } = "01";
        public string StoreSection { get; set; } = "Main Floor"; // e.g., Bar, Upstairs
        public string CurrencySymbol { get; set; } = "£";

        // --- Operational Settings ---
        public decimal AdultCoverPrice { get; set; } = 0.00m;
        public decimal ChildCoverPrice { get; set; } = 0.00m;
        public bool PrintReceiptOnPayment { get; set; } = true;
        public string FooterMessage { get; set; } = "Thank you for dining with us!";

        [NotMapped]
        public string AdultCoverLabel { get; set; } = "Adults";
        
        [NotMapped]
        public string ChildCoverLabel { get; set; } = "Children";
    }
}
