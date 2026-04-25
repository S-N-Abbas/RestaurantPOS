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
        public async Task<bool> TableNumberExistsAsync(int number, int? excludeId = null)
        {
            return await _context.Tables
                .AnyAsync(t =>
                    t.Number == number &&
                    t.IsActive &&
                    (excludeId == null || t.Id != excludeId));
        }

        public async Task<Table> SaveTableAsync(int? id, int number)
        {
            if (number <= 0)
                throw new ArgumentException("Table number must be greater than zero.");

            // Guard duplicate
            if (await TableNumberExistsAsync(number, excludeId: id))
                throw new InvalidOperationException(
                    $"Table {number} already exists.");

            Table table;

            if (id == null)
            {
                table = new Table { Number = number, IsActive = true };
                _context.Tables.Add(table);
            }
            else
            {
                table = await _context.Tables.FindAsync(id)
                    ?? throw new InvalidOperationException("Table not found.");
                table.Number = number;
            }

            await _context.SaveChangesAsync();
            return table;
        }

        public async Task DeleteTableAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id)
                ?? throw new InvalidOperationException("Table not found.");

            // Check for active orders on this table
            bool hasActiveOrder = await _context.Orders
                .AnyAsync(o => o.TableId == id && o.ClosedAt == null);

            if (hasActiveOrder)
                throw new InvalidOperationException(
                    $"Table {table.Number} has an active order. " +
                    "Please close or transfer the order first.");

            // Soft delete — preserves historical order records
            table.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
