using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RestaurantPOS.Services
{
    public class UserService : IUserService
    {
        private readonly PosDbContext _context;

        public UserService(PosDbContext context)
        {
            _context = context;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users
                           .AsNoTracking()
                           .OrderBy(u => u.Username)
                           .ToList();
        }

        public async Task<bool> ValidatePinAsync(int userId, string pin)
        {
            var user = await _context.Users
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            //var hash = HashPin(pin);
            var hash = pin;
            return user.PasscodeHash == hash;
        }

        /* ✅ PIN HASHING */

        private static string HashPin(string pin)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(pin);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToHexString(hash);
        }
    }
}
