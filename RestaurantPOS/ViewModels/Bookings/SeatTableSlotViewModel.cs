using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Bookings
{
    /// <summary>
    /// Represents a single table card in the seat picker overlay.
    /// Read-only — parent BookingsViewModel handles the selection command.
    /// </summary>
    public class SeatTableSlotViewModel
    {
        public int TableNumber { get; }
        public bool IsOccupied { get; }
        public bool IsFree => !IsOccupied;

        public SeatTableSlotViewModel(int tableNumber, bool isOccupied)
        {
            TableNumber = tableNumber;
            IsOccupied = isOccupied;
        }
    }
}
