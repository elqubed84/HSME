using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace HSEM.Converters
{
    public class StarColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null) return Colors.LightGray;

                if (!decimal.TryParse(value.ToString(), out var score)) return Colors.LightGray;
                if (!int.TryParse(parameter.ToString(), out var starNumber)) return Colors.LightGray;

                // اختيار اللون حسب الدرجة
                string colorHex = score >= starNumber
                    ? (score < 3 ? "#dc3545" : score < 4 ? "#ffc107" : "#28a745")
                    : "#D3D3D3";

                return Color.FromArgb(colorHex);
            }
            catch
            {
                return Colors.LightGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ScoreToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return "0%";

                if (!decimal.TryParse(value.ToString(), out var score)) return "0%";

                var percentage = Math.Round((score / 5m) * 100, 1);
                return $"{percentage}%";
            }
            catch
            {
                return "0%";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
