using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class GpsLocationInfo
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }   // بالمتر
        public double Speed { get; set; }       // m/s
        public DateTime Timestamp { get; set; }
        public bool IsMocked { get; set; }
    }

}
