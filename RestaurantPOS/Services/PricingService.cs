using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class PricingService : IPricingService
    {
        public decimal AdultCoverRate => 500m;
        public decimal ChildCoverRate => 300m;

        public decimal CalculateCoverCharge(Order order)
        {
            if (order == null)
                return 0;

            return (order.AdultCovers * AdultCoverRate)
                 + (order.ChildCovers * ChildCoverRate);
        }
    }
}
