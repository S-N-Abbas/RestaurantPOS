using RestaurantPOS.ViewModels.Cover;
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

namespace RestaurantPOS.UI.Views.Cover
{
    /// <summary>
    /// Interaction logic for CoverSelector.xaml
    /// </summary>
    public partial class CoverSelectorView : UserControl
    {
        public CoverSelectorView()
        {
            InitializeComponent();
        }

        private void AdultsBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is CoverSelectorViewModel vm)
                vm.SetActiveField(CoverField.Adults);
        }

        private void ChildrenBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is CoverSelectorViewModel vm)
                vm.SetActiveField(CoverField.Children);
        }

    }
}
