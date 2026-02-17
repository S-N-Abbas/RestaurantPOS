using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class AuthService
    {
        private readonly PosDbContext _context;
        private readonly PasscodeService _passcodeService;

        public AuthService(PosDbContext context, PasscodeService passcodeService)
        {
            _context = context;
            _passcodeService = passcodeService;
        }

        public async Task<User?> LoginWithPinAsync(int userId, string pin)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null)
                return null;

            if (!_passcodeService.Verify(pin, user.PasscodeHash))
                return null;

            return user;
        }
    }

}
