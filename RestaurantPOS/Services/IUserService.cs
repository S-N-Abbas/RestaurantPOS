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

        void CreateUser(User user);

        void UpdateUser(User user);

        void DeleteUser(int userId);
    }

}
