using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace HSEM.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        private static readonly CultureInfo ArEg = CultureInfo.GetCultureInfo("ar-EG");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            if (value is decimal d) return d.ToString("C", ArEg);
            if (value is double db) return ((decimal)db).ToString("C", ArEg);
            if (decimal.TryParse(value.ToString(), out var parsed)) return parsed.ToString("C", ArEg);
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}