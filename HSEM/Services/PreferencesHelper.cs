using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public class CompanyPrefs
    {
        public double CompanyLat { get; set; }
        public double CompanyLng { get; set; }
        public double RadiusMeters { get; set; }
        public string UserId { get; set; }
    }

    public static class PreferencesHelper
    {
        public static CompanyPrefs LoadCompanyPrefs()
        {
            try
            {
                var lat = Preferences.Get("CompanyLatitude", 0d);
                var lng = Preferences.Get("CompanyLongitude", 0d);
                var r = Preferences.Get("CompanyRadius", 200d);
                var uid = Preferences.Get("UserId", "");
                if (lat == 0 && lng == 0) return null;
                return new CompanyPrefs { CompanyLat = lat, CompanyLng = lng, RadiusMeters = r, UserId = uid };
            }
            catch { return null; }
        }
    }
}
