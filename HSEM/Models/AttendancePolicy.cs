using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class AttendancePolicy
    {
        public double CompanyLatitude { get; set; }
        public double CompanyLongitude { get; set; }
        public double AllowedRadiusMeters { get; set; }

        public bool RequireWifi { get; set; }
        public string AllowedWifiBssid { get; set; } = "";

        public bool AllowMockLocation { get; set; }
    }

}
