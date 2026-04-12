using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class PermissionRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("الموظف")]
        public string? UserId { get; set; }


        [DisplayName("نوع الإذن")]
        public PermissionType Type { get; set; } = PermissionType.General;

        [DisplayName("نطاق الإذن")]
        public PermissionScope Scope { get; set; } = PermissionScope.Custom;

        [DisplayName("تاريخ/وقت البداية")]
        public DateTime StartDateTime { get; set; }

        [DisplayName("تاريخ/وقت النهاية")]
        public DateTime EndDateTime { get; set; }

        [DisplayName("عدد الساعات المطلوبة")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RequestedHours { get; set; } = 0m;

        [DisplayName("حالة الإذن")]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        [DisplayName("بواسطة")]
        public string? CreatedBy { get; set; }

        [DisplayName("تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [DisplayName("تاريخ التحديث")]
        public DateTime UpdatedAt { get; set; }

        [DisplayName("قرار المدير")]
        public string? DecisionBy { get; set; }

        [DisplayName("تاريخ القرار")]
        public DateTime? DecisionAt { get; set; }

        [DisplayName("ملاحظات المدير")]
        public string? ManagerNotes { get; set; }

    }

}
