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
        Task<Table> SaveTableAsync(int? id, int number);       // create or update
        Task DeleteTableAsync(int id);                          // soft delete
        Task<bool> TableNumberExistsAsync(int number, int? excludeId = null);
    }
}
