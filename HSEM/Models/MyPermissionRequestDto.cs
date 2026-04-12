using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class MyPermissionRequestDto
    {
        public int Id { get; set; }
        public PermissionType Type { get; set; }
        public PermissionScope Scope { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal RequestedHours { get; set; }
        public LeaveStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string StatusText => Status switch
        {
            LeaveStatus.Pending => "قيد الانتظار",
            LeaveStatus.Approved => "مقبول",
            LeaveStatus.Rejected => "مرفوض",
            _ => "غير معروف"
        };

        public string TypeText => GetDisplayName(Type);
        public string ScopeText => GetDisplayName(Scope);
        public string DateText => $"{StartDateTime:yyyy/MM/dd HH:mm} - {EndDateTime:HH:mm}";
        private static string GetDisplayName(Enum value)
        {
            var attr = value.GetType()
                            .GetField(value.ToString())
                            .GetCustomAttribute<DisplayAttribute>();
            return attr?.Name ?? value.ToString();
        }
    }
    public enum PermissionType
    {
        [Display(Name = "إذن تأخير دخول")]
        LateArrival,

        [Display(Name = "إذن خروج مبكر")]
        EarlyLeave,

        [Display(Name = "مأمورية / عمل خارجى")]
        Mission,

        [Display(Name = "إذن عام")]
        General
    }

    public enum PermissionScope
    {
        [Display(Name = "بداية الدوام")]
        StartOfDay,

        [Display(Name = "نهاية الدوام")]
        EndOfDay,

        [Display(Name = "وقت مخصص")]
        Custom
    }
    public enum LeaveStatus
    {
        [Display(Name = "قيد المراجعة")]
        Pending,

        [Display(Name = "مقبولة")]
        Approved,

        [Display(Name = "مرفوضة")]
        Rejected
    }
}
