using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.ViewModels.Login
{
    public class UserViewModel
    {
        private readonly User _user;

        public int Id => _user.Id;
        public string Username => _user.Username;

        public UserViewModel(User user)
        {
            _user = user;
        }
    }

}
