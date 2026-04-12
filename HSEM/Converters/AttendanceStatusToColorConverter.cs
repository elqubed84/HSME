using HSEM.Models;
using HSEM.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Converters
{
    public class AttendanceStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is AttendanceDayVM day)
            {
                if (day.IsWeeklyOff)
                    return Color.FromArgb("#E0E0E0"); // إجازة

                if (day.IsAbsent)
                    return Color.FromArgb("#FFCDD2"); // غياب

                if (day.LatenessMinutes > 0)
                    return Color.FromArgb("#FFF9C4"); // تأخير

                if (day.EarlyLeaveMinutes > 0)
                    return Color.FromArgb("#FFE0B2"); // انصراف مبكر

                if (day.OvertimeMinutes > 0)
                    return Color.FromArgb("#C8E6C9"); // أوفر تايم

                return Color.FromArgb("#E3F2FD"); // طبيعي
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
           System.Globalization.CultureInfo culture) => null;
    }


}
