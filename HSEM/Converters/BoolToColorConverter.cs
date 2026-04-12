using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace HSEM.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isWeekend && isWeekend)
                return Color.FromArgb("#E0E0E0"); // لون رمادي للإجازة
            return Color.FromArgb("#FFFFFF"); // لون الأيام العادية
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
