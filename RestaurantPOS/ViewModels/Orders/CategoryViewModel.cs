using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Orders
{
    public class CategoryViewModel : ViewModelBase
    {
        public int Id { get; }
        public string Name { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            private set => SetProperty(ref _isSelected, value);
        }

        public void RaiseIsSelected(int? selectedId)
            => IsSelected = selectedId == Id;

        public CategoryViewModel(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
