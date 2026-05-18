using RestaurantPOS.ViewModels.Payments;
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

namespace RestaurantPOS.UI.Views.Payments
{
    /// <summary>
    /// Interaction logic for PaymentView.xaml
    /// </summary>
    public partial class PaymentView : UserControl
    {
        public PaymentView()
        {
            InitializeComponent();
            // Allow the control to receive keyboard focus
            Focusable = true;
            Loaded += (s, e) => Focus(); // Auto-focus when view loads
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Ensure the DataContext is our PaymentViewModel
            if (DataContext is PaymentViewModel vm)
            {
                // Handle Number Row (0-9)
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    string digit = (e.Key - Key.D0).ToString();
                    if (vm.AppendDigitCommand.CanExecute(digit))
                        vm.AppendDigitCommand.Execute(digit);

                    e.Handled = true;
                }
                // Handle Numpad Row (0-9)
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                    string digit = (e.Key - Key.NumPad0).ToString();
                    if (vm.AppendDigitCommand.CanExecute(digit))
                        vm.AppendDigitCommand.Execute(digit);

                    e.Handled = true;
                }
                // Handle Backspace
                else if (e.Key == Key.Back)
                {
                    if (vm.BackspaceCommand.CanExecute(null))
                        vm.BackspaceCommand.Execute(null);

                    e.Handled = true;
                }
                // Handle Escape or Delete to clear the amount
                else if (e.Key == Key.Escape || e.Key == Key.Delete)
                {
                    if (vm.ClearAmountCommand.CanExecute(null))
                        vm.ClearAmountCommand.Execute(null);

                    e.Handled = true;
                }
                // Handle Enter to execute Pay action
                else if (e.Key == Key.Enter)
                {
                    if (vm.PayCommand.CanExecute(null))
                        vm.PayCommand.Execute(null);

                    e.Handled = true;
                }
            }

            base.OnPreviewKeyDown(e);
        }
    }
}
