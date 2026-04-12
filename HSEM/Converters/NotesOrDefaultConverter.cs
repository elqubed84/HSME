using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace HSEM.Converters
{
    public class NotesOrDefaultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;

            if (string.IsNullOrWhiteSpace(text))
                return "لا توجد ملاحظات";

            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
