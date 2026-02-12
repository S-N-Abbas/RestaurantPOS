using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface IPricingService
    {
        decimal AdultCoverRate { get; }
        decimal ChildCoverRate { get; }
        decimal CalculateCoverCharge(Order order);
    }
}
