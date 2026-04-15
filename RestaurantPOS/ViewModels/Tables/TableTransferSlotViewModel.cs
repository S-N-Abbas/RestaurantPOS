using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Tables
{
    /// <summary>
    /// Represents a single table card in the transfer picker.
    /// Read-only — no commands, parent handles selection.
    /// </summary>
    public class TableTransferSlotViewModel
    {
        public int TableNumber { get; }
        public bool IsOccupied { get; }
        public bool IsFree => !IsOccupied;

        public TableTransferSlotViewModel(int tableNumber, bool isOccupied)
        {
            TableNumber = tableNumber;
            IsOccupied = isOccupied;
        }
    }
}
