using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class AttendanceDayVM
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public int WorkedMinutes { get; set; }
        public int LatenessMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public bool IsAbsent { get; set; }
        public int OvertimeMinutes { get; set; }
        public decimal DeductionAmount { get; set; }

        public bool IsWeeklyOff { get; set; }

        public string Status =>
            IsWeeklyOff ? "إجازة" :
            IsAbsent ? "غائب" : "حاضر";

        public string DateText => Date.ToString("yyyy/MM/dd");
        public string InTimeText => InTime?.ToString("HH:mm") ?? "-";
        public string OutTimeText => OutTime?.ToString("HH:mm") ?? "-";
    }

    public class AttendanceResponseDto
    {
        public bool HasData { get; set; }
        public string? Message { get; set; }
        public List<AttendanceDayVM> Data { get; set; } = new();
    }

}
