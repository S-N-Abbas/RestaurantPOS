using RestaurantPOS.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RestaurantPOS.UI.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();
        }

        // Inline Menu Editor
        private void ProductName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is OrderViewModel vm)
                vm.MenuEditor.FocusNameCommand.Execute(null);
        }
        private void ProductPrice_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is OrderViewModel vm)
                vm.MenuEditor.FocusPriceCommand.Execute(null);
        }

        // Open Item Editor
        private void NameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is OrderViewModel vm)
                vm.OpenItemEditor.FocusNameCommand.Execute(null);
        }

        private void PriceBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is OrderViewModel vm)
                vm.OpenItemEditor.FocusPriceCommand.Execute(null);
        }
    }
}
