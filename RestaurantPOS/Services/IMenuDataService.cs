using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public interface IMenuDataService
    {
        Task<IReadOnlyList<Category>> GetCategoriesAsync();
        Task<IReadOnlyList<MenuProduct>> GetProductsAsync();
        void InvalidateCache();
    }
}
