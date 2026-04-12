using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using HSEM.Models;

namespace HSEM.Converters
{
    public class TotalAfterAdditionsConverter : IValueConverter
    {
        private static readonly CultureInfo ArEg = CultureInfo.GetCultureInfo("ar-EG");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MonthlyPayroll p)
                return string.Empty;

            var total = p.BaseAfterAbsence + p.Allowances + p.ExtraHoursAmount;
            return total.ToString("C", ArEg);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}