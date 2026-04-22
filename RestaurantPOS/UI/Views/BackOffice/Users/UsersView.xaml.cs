using RestaurantPOS.ViewModels.BackOffice.Users;
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

namespace RestaurantPOS.UI.Views.BackOffice.Users
{
    /// <summary>
    /// Interaction logic for UsersView.xaml
    /// </summary>
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
        }

        private void UserPinBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Since PasswordBox isn't bindable for security, update VM manually
            if (this.DataContext is UsersViewModel vm)
            {
                vm.EditorPin = ((PasswordBox)sender).Password;
            }
        }

        private void UserPinBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is UsersViewModel vm)
            {
                vm.FocusPinCommand.Execute(null);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is UsersViewModel vm)
            {
                vm.FocusNameCommand.Execute(null);
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is UsersViewModel vm)
            {
                vm.FocusSearchCommand.Execute(null);
            }
        }
    }
}
