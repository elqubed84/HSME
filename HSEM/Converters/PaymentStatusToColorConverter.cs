using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace HSEM.Converters
{
    public class PaymentStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCompleted = false;

            if (value is bool b)
                isCompleted = b;
            else if (value is int i)  // لو API بيرجع 1 و 0
                isCompleted = i == 1;
            else if (value is string s) // لو API بيرجع "true"/"false"
                bool.TryParse(s, out isCompleted);

            return isCompleted ? Colors.Green : Colors.DarkRed;
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
