using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Tables
{
    public class TableViewModel : ViewModelBase
    {
        public int Number { get; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private bool _hasOrder;
        public bool HasOrder
        {
            get => _hasOrder;
            set => SetProperty(ref _hasOrder, value);
        }

        public TableViewModel(
            int number,
            ITableSessionService tableSession,
            OrderStore orderStore)
        {
            Number = number;

            IsActive = tableSession.CurrentTable == number;

            tableSession.TableChanged += t =>
                IsActive = (t == Number);

            HasOrder = orderStore.HasOrder(number);

            orderStore.OrderStateChanged += t =>
            {
                if (t == Number)
                    HasOrder = orderStore.HasOrder(Number);
            };
        }
    }
}
