using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public static class GeoTrackingService
    {
        static CancellationTokenSource _cts;
        static bool alreadyOutside = false;
        public static async Task StartAsync()
        {
            if (_cts != null)
                return;

            _cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    await CheckLocation();

                    await Task.Delay(TimeSpan.FromMinutes(3));
                }

            }, _cts.Token);
        }

        public static void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }

        static async Task CheckLocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location == null ||
                    DateTimeOffset.UtcNow - location.Timestamp > TimeSpan.FromMinutes(5))
                {
                    location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(
                            GeolocationAccuracy.Low,
                            TimeSpan.FromSeconds(5)));
                }

                if (location == null)
                    return;

                if (location == null)
                    return;

                double employeeLat = location.Latitude;
                double employeeLng = location.Longitude;

                double companyLat = Preferences.Get("CompanyLatitude", 0d);
                double companyLng = Preferences.Get("CompanyLongitude", 0d);
                double radius = Preferences.Get("CompanyRadius", 200d);

                double distance = Haversine(employeeLat, employeeLng, companyLat, companyLng);

                if (distance > radius && !alreadyOutside)
                {
                    alreadyOutside = true;
                    await SendExitEvent(distance);
                }

                if (distance <= radius)
                {
                    alreadyOutside = false;
                }
            }
            catch
            {
            }
        }

        static async Task SendExitEvent(double distance)
        {
            try
            {
                var client = HttpClientFactory.Instance;

                var payload = new
                {
                    UserId = Preferences.Get("UserId", ""),
                    Distance = distance,
                    Time = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(payload);

                await client.PostAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/location/exit",
                    new StringContent(json, Encoding.UTF8, "application/json"));
            }
            catch
            {
            }
        }

        static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3;
            var φ1 = lat1 * Math.PI / 180;
            var φ2 = lat2 * Math.PI / 180;
            var Δφ = (lat2 - lat1) * Math.PI / 180;
            var Δλ = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
