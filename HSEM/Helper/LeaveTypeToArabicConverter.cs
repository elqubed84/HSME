using HSEM.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Helper
{
    public class LeaveTypeToArabicConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is LeaveType t ? t.ToArabic() : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public static class LeaveTypeExtensions
    {
        public static string ToArabic(this LeaveType type)
        {
            var memInfo = type.GetType().GetMember(type.ToString());
            if (memInfo.Length > 0)
            {
                var displayAttr = memInfo[0].GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null)
                    return displayAttr.Name;
            }
            return type.ToString();
        }
        //    public static string ToArabic(this LeaveType type)
        //    {
        //        return type switch
        //        {
        //            LeaveType.Annual => "إجازة سنوية",
        //            LeaveType.Sick => "إجازة مرضية",
        //            LeaveType.Casual => "إجازة عارضة",
        //            LeaveType.Other => "أخرى",
        //            _ => type.ToString()
        //        };
        //    }
        //}


    }
}
