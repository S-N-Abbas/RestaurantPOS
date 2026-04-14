using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Orders
{
    public class MenuItemViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        public int Id { get; }
        public string Name { get; }
        public decimal Price { get; }
        public int CategoryId { get; }

        public string CurrencySymbol => _settingsService.Settings.CurrencySymbol;

        public MenuItemViewModel(int id, string name, decimal price, int categoryId, SettingsService settingsService)
        {
            Id = id;
            Name = name;
            Price = price;
            CategoryId = categoryId;
            _settingsService = settingsService;

            _settingsService.SettingsChanged += () =>
            {
                OnPropertyChanged(nameof(CurrencySymbol));
            };
        }
    }
}
