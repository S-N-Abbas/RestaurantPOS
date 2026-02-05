using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantPOS.UI.Converters
{

    public class BoolToVisibilityConverter : IValueConverter
    {
        // If true → Visible, false → Collapsed
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Optional: allow "invert" via parameter
                if (parameter?.ToString() == "Invert")
                    boolValue = !boolValue;

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;

                if (parameter?.ToString() == "Invert")
                    result = !result;

                return result;
            }

            return false;
        }
    }
}