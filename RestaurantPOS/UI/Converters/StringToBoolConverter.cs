using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RestaurantPOS.UI.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        // Convert: String property -> Boolean (IsChecked)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        // ConvertBack: Boolean (Clicked) -> String property
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
                return parameter.ToString();

            return Binding.DoNothing;
        }
    }
}
