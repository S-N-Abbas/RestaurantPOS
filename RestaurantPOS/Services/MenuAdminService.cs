using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Data;

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
                category = new Category { Name = name.Trim(), IsActive = true };
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
    }
}