using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Domain.Entities
{
    public class Table
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
