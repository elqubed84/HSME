using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class LocalAttendanceRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Action { get; set; }       // IN أو OUT
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public bool IsMockLocation { get; set; }
        public DateTime DeviceTime { get; set; }
        public bool IsSynced { get; set; }
        public bool IsOfflineRecord { get; set; }  // ✅ لتمييز السجلات المحفوظة محلياً
        public double CompanyLat { get; set; }
        public double CompanyLng { get; set; }
        public double AllowedRadius { get; set; }// اتبعت للسيرفر ولا لسه

    }
}
