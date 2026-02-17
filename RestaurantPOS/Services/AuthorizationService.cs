using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class AuthorizationService
    {
        private readonly UserSessionService _session;

        public AuthorizationService(UserSessionService session)
        {
            _session = session;
        }

        public bool HasAccess(params UserRole[] roles)
        {
            if (_session.CurrentUser == null)
                return false;

            return roles.Contains(_session.CurrentUser.Role);
        }
    }

}
