using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface ITableSessionService
    {
        int CurrentTable { get; }

        event Action<int>? TableChanged;

        void SwitchTable(int tableNumber);
    }
}
