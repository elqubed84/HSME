using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class OfflineAttendance
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // وقت الإنشاء الحقيقي (UTC)
        public DateTime CreatedAtUtc { get; set; }

        // وقت الجهاز (للعرض فقط)
        public DateTime LocalTime { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public bool IsMockLocation { get; set; }

        public string SSID { get; set; } = "";
        public string BSSID { get; set; } = "";

        // IN / OUT
        public string Action { get; set; } = "";

        // Integrity
        public string Hash { get; set; } = "";
    }
}
