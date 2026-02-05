using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class TableSessionService : ITableSessionService
    {
        public int CurrentTable { get; private set; }

        public event Action<int>? TableChanged;

        public TableSessionService()
        {
            CurrentTable = 1; // default
        }

        public void SwitchTable(int tableNumber)
        {
            if (CurrentTable == tableNumber)
                return;

            CurrentTable = tableNumber;
            TableChanged?.Invoke(tableNumber);
        }
    }
}
