using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface IPaymentService
    {
        Task AddPaymentAsync(
            Order order,
            PaymentMethod method,
            decimal amount);

        Task<bool> IsOrderFullyPaidAsync(Order order);

        Task CloseIfPaidAsync(Order order);
    }
}
