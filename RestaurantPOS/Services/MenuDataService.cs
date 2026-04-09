using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RestaurantPOS.Services
{
    public class MenuDataService : IMenuDataService
    {
        private readonly PosDbContext _db;

        private IReadOnlyList<Category>? _categoriesCache;
        private IReadOnlyList<MenuProduct>? _productsCache;

        public MenuDataService(PosDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
        {
            if (_categoriesCache != null)
                return _categoriesCache;

            _categoriesCache = await _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            return _categoriesCache;
        }

        public async Task<IReadOnlyList<MenuProduct>> GetProductsAsync()
        {
            if (_productsCache != null)
                return _productsCache;

            _productsCache = await _db.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();

            return _productsCache;
        }

        public void InvalidateCache()
        {
            _categoriesCache = null;
            _productsCache = null;
        }
    }
}
