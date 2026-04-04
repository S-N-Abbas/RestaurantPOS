using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface IOrderContextService
    {
        int CurrentContext{ get; }
        OrderType CurrentOrderType { get; }

        event Action<int>? ContextChanged;

        void SwitchContext(int contextId);
    }
}
