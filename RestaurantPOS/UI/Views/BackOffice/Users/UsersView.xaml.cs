using RestaurantPOS.ViewModels.BackOffice.Users;
using RestaurantPOS.ViewModels.Login;
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
            // Enable the control to receive direct global keyboard focus
            Focusable = true;
            Loaded += (s, e) => Focus(); // Automatically focus when the view displays
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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Check ensures commands match your ViewModel signature
            if (DataContext is UsersViewModel vm)
            {
                // 1. Handle Letter Keys (A-Z)
                if (e.Key >= Key.A && e.Key <= Key.Z)
                {
                    // Convert Key to corresponding character
                    char letter = (char)('A' + (e.Key - Key.A));
                    ExecuteCommand(vm.KeyCommand, letter.ToString());
                    e.Handled = true;
                }
                // 2. Handle Top Row Number Keys (0-9)
                else if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    string digit = (e.Key - Key.D0).ToString();
                    ExecuteCommand(vm.KeyCommand, digit);
                    e.Handled = true;
                }
                // 3. Handle Numpad Keys (0-9)
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                    string digit = (e.Key - Key.NumPad0).ToString();
                    ExecuteCommand(vm.KeyCommand, digit);
                    e.Handled = true;
                }
                // 4. Handle Backspace (← Button)
                else if (e.Key == Key.Back)
                {
                    ExecuteCommand(vm.BackspaceCommand, null);
                    e.Handled = true;
                }
                // 5. Handle Escape or Delete (Clear "C" Button)
                else if (e.Key == Key.Escape || e.Key == Key.Delete)
                {
                    ExecuteCommand(vm.ClearEditorCommand, null);
                    e.Handled = true;
                }
                
            }

            base.OnPreviewKeyDown(e);
        }

        private void ExecuteCommand(ICommand command, object parameter)
        {
            if (command != null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }

    }
}
