using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class UserSessionService
    {
        public User? CurrentUser { get; private set; }

        public bool IsLoggedIn => CurrentUser != null;

        public event Action? UserChanged;

        public void SetUser(User user)
        {
            CurrentUser = user;
            UserChanged?.Invoke();
        }

        public void Logout()
        {
            CurrentUser = null;
            UserChanged?.Invoke();
        }
    }

}
