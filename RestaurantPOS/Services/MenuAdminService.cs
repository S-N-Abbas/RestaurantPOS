using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;
using System.Security.RightsManagement;

namespace RestaurantPOS.Services
{
    public class MenuAdminService : IMenuAdminService
    {
        private readonly PosDbContext _db;
        private readonly IMenuDataService _menuDataService;

        public MenuAdminService(PosDbContext db, IMenuDataService menuDataService)
        {
            _db = db;
            _menuDataService = menuDataService;
        }

        public async Task<Category> SaveCategoryAsync(int? id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name is required.");

            Category category;

            if (id == null)
            {
                int displayOrder = await _db.Categories.CountAsync() + 1; // New category goes to the end
                category = new Category { Name = name.Trim(), IsActive = true, DisplayOrder = displayOrder };
                _db.Categories.Add(category);
            }
            else
            {
                category = await _db.Categories.FindAsync(id)
                    ?? throw new InvalidOperationException("Category not found.");
                category.Name = name.Trim();
            }

            await _db.SaveChangesAsync();
            _menuDataService.InvalidateCache(); // ✅ order screen picks it up next load
            return category;
        }

        public async Task MoveCategoryAsync(int categoryId, bool moveUp)
        {
            // Load all active categories ordered by DisplayOrder
            var categories = await _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)   // stable tiebreak
                .ToListAsync();

            var index = categories.FindIndex(c => c.Id == categoryId);

            if (index < 0) return;  // not found

            // Calculate neighbour index
            int neighbourIndex = moveUp ? index - 1 : index + 1;

            // Guard boundaries
            if (neighbourIndex < 0 || neighbourIndex >= categories.Count)
                return;

            var current = categories[index];
            var neighbour = categories[neighbourIndex];

            // ✅ Swap DisplayOrder values
            (current.DisplayOrder, neighbour.DisplayOrder) =
                (neighbourIndex, index);

            await _db.SaveChangesAsync();
            _menuDataService.InvalidateCache();
        }

        public async Task<MenuProduct> SaveProductAsync(
            int? id, string name, decimal price, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.");

            if (price < 0)
                throw new ArgumentException("Price cannot be negative.");

            MenuProduct product;

            if (id == null)
            {
                product = new MenuProduct
                {
                    Name = name.Trim(),
                    Price = price,
                    CategoryId = categoryId,
                    IsActive = true
                };
                _db.Products.Add(product);
            }
            else
            {
                product = await _db.Products.FindAsync(id)
                    ?? throw new InvalidOperationException("Product not found.");
                product.Name = name.Trim();
                product.Price = price;
                product.CategoryId = categoryId;
            }

            await _db.SaveChangesAsync();
            _menuDataService.InvalidateCache(); // ✅ fresh load next time
            return product;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new InvalidOperationException("Category not found.");

            // Soft delete — hide from order screen but preserve history
            category.IsActive = false;
            foreach (var p in category.Products)
                p.IsActive = false;

            await _db.SaveChangesAsync();
            _menuDataService.InvalidateCache();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _db.Products.FindAsync(id)
                ?? throw new InvalidOperationException("Product not found.");

            // Soft delete — preserves it in historical order records
            product.IsActive = false;

            await _db.SaveChangesAsync();
            _menuDataService.InvalidateCache();
        }
    }
}