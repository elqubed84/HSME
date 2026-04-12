using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class WifiNetworkInfo
    {
        public string SSID { get; set; } = "";
        public string BSSID { get; set; } = "";
        public int SignalLevel { get; set; }
    }

}
