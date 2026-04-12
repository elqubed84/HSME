using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using HSEM.Models;

namespace HSEM.ViewModels
{
    public class LeaveStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LeaveStatus status)
            {
                return status switch
                {
                    LeaveStatus.Pending => Colors.Orange,
                    LeaveStatus.Approved => Colors.Green,
                    LeaveStatus.Rejected => Colors.Red,
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
