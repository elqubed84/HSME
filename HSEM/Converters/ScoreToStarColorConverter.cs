using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace HSEM.Converters
{
    public class ScoreToStarColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Colors.LightGray;

            int score = System.Convert.ToInt32(value);
            int starIndex = System.Convert.ToInt32(parameter);

            return starIndex <= score ? Colors.Gold : Colors.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
