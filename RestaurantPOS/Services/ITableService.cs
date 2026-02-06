using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Services
{
    public interface ITableService
    {
        Task<IReadOnlyList<Table>> GetAllAsync();
    }
}
