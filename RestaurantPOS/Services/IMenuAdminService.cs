using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RestaurantPOS.Services
{
    public interface IMenuAdminService
    {
        Task<Category> SaveCategoryAsync(int? id, string name);
        Task DeleteCategoryAsync(int id);

        Task<MenuProduct> SaveProductAsync(int? id, string name, decimal price, int categoryId);
        Task DeleteProductAsync(int id);
    }
}

