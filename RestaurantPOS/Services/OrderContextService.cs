using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class OrderContextService : IOrderContextService
    {
        public int CurrentContext { get; private set; }

        public event Action<int>? ContextChanged;

        public OrderContextService()
        {
            CurrentContext = 1; // default
        }

        public void SwitchContext(int contextId)
        {
            if (CurrentContext == contextId)
                return;

            CurrentContext = contextId;
            ContextChanged?.Invoke(contextId);
        }
    }
}
