using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class AttendancePolicyDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusMeters { get; set; }

        public string CompanyWifiSSID { get; set; }
        public string CompanyWifiBSSID { get; set; }

        public bool RequireWifi { get; set; }
        public bool RequireGps { get; set; }

        public DateTime LastUpdated { get; set; }
    }

}
