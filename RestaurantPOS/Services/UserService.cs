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

        // ✅ CREATE

        public void CreateUser(User user)
        {
            if (user == null)
                return;

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        // ✅ UPDATE

        public void UpdateUser(User user)
        {
            if (user == null)
                return;

            var existingUser = _context.Users.Find(user.Id);

            if (existingUser == null)
                return;

            existingUser.Username = user.Username;
            existingUser.Role = user.Role;
            existingUser.PasscodeHash = user.PasscodeHash;
            existingUser.IsActive = user.IsActive;

            _context.SaveChanges();
        }

        // ✅ DELETE

        public void DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);

            if (user == null)
                return;

            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}
