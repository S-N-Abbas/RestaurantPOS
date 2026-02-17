using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Domain.Entities
{

    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        Cashier = 3
    }

    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasscodeHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
