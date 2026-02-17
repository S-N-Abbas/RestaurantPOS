using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        Task<bool> ValidatePinAsync(int userId, string pin);
    }

}
