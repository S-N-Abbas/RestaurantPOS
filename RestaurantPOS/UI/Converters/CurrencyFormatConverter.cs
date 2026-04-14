using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RestaurantPOS.UI.Converters
{
    public class CurrencyFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return "";

            // values[0] is the Decimal Amount
            // values[1] is the Currency Symbol string
            if (values[0] is decimal amount && values[1] is string symbol)
            {
                return $"{symbol}{amount:N2}";
            }

            return "0.00";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
