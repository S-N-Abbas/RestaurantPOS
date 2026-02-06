using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class TableService : ITableService
    {
        private readonly PosDbContext _context;

        public TableService(PosDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Table>> GetAllAsync()
        {
            return await _context.Tables
                .Where(t => t.IsActive)
                .OrderBy(t => t.Number)
                .ToListAsync();
        }
    }
}
