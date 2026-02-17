using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class PasscodeService
    {
        public string Hash(string password)
        {
            //return BCrypt.Net.BCrypt.HashPassword(password);
            return password;
        }

        public bool Verify(string password, string hash)
        {
            //return BCrypt.Net.BCrypt.Verify(password, hash);
            return password == hash;
        }
    }

}
