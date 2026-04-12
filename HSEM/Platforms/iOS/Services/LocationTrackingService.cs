using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using UserNotifications;
using Microsoft.Maui.Storage;
using HSEM.Models;
using HSEM.Services;

namespace HSEM.Platforms.iOS.Services
{
    /// <summary>
    /// خدمة تتبع الموقع المحسّنة لـ iOS
    /// تستخدم Region Monitoring + Significant Location Changes
    /// مع دعم Offline Sync
    /// </summary>
    public class LocationTrackingService : CLLocationManagerDelegate
    {
        private CLLocationManager locationManager;
        private readonly double companyLat;
        private readonly double companyLng;
        private readonly double radiusMeters;

        private bool alreadyOutside = false;
        private CLCircularRegion companyRegion;

        public LocationTrackingService(double companyLat, double companyLng, double radiusMeters)
        {
            this.companyLat = companyLat;
            this.companyLng = companyLng;
            this.radiusMeters = radiusMeters;
        }

        #region Start/Stop Tracking

        public void StartTracking()
        {
            locationManager = new CLLocationManager
            {
                Delegate = this,
                AllowsBackgroundLocationUpdates = true,
                PausesLocationUpdatesAutomatically = false,
                DesiredAccuracy = CLLocation.AccuracyBest,
                DistanceFilter = 50 // Update every 50 meters
            };

            // طلب صلاحيات دائمًا
            locationManager.RequestAlwaysAuthorization();

            // ✅ 1. بدء التتبع العادي
            locationManager.StartUpdatingLocation();

            // ✅ 2. تفعيل Significant Location Changes (أقل استهلاك للبطارية)
            if (CLLocationManager.SignificantLocationChangeMonitoringAvailable)
            {
                locationManager.StartMonitoringSignificantLocationChanges();
                Console.WriteLine("✅ Significant location monitoring started");
            }

            // ✅ 3. Region Monitoring (تنبيه عند الدخول/الخروج من نطاق الشركة)
            SetupRegionMonitoring();

            Console.WriteLine("✅ Location tracking started");
        }

        public void StopTracking()
        {
            if (locationManager == null) return;

            locationManager.StopUpdatingLocation();
            locationManager.StopMonitoringSignificantLocationChanges();

            if (companyRegion != null)
            {
                locationManager.StopMonitoring(companyRegion);
            }

            locationManager?.Dispose();
            locationManager = null;

            Console.WriteLine("⏹️ Location tracking stopped");
        }

        #endregion

        #region Region Monitoring

        private void SetupRegionMonitoring()
        {
            if (!CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
            {
                Console.WriteLine("⚠️ Region monitoring not available");
                return;
            }

            // إنشاء منطقة دائرية حول الشركة
            companyRegion = new CLCircularRegion(
                new CLLocationCoordinate2D(companyLat, companyLng),
                radiusMeters,
                "CompanyRegion");

            companyRegion.NotifyOnEntry = true;  // تنبيه عند الدخول
            companyRegion.NotifyOnExit = true;   // تنبيه عند الخروج

            locationManager.StartMonitoring(companyRegion);
            Console.WriteLine($"✅ Region monitoring started: {radiusMeters}m radius");
        }

        // ✅ صحيح: RegionEntered
        public override void RegionEntered(CLLocationManager manager, CLRegion region)
        {
            if (region.Identifier == "CompanyRegion")
            {
                Console.WriteLine("🏢 Entered company region");
                alreadyOutside = false;

                // إرسال حدث الدخول
                var distance = 0.0; // داخل النطاق
                SendEventInBackgroundAsync("دخول نطاق العمل", distance);
                ShowNotification("🏢 دخلت نطاق الشركة");
            }
        }

        // ✅ صحيح: RegionLeft
        public override void RegionLeft(CLLocationManager manager, CLRegion region)
        {
            if (region.Identifier == "CompanyRegion")
            {
                Console.WriteLine("🚶 Exited company region");
                alreadyOutside = true;

                // حساب المسافة الفعلية
                var currentLocation = manager.Location;
                if (currentLocation != null)
                {
                    var distance = Haversine(
                        currentLocation.Coordinate.Latitude,
                        currentLocation.Coordinate.Longitude,
                        companyLat, companyLng);

                    SendEventInBackgroundAsync("خروج عن نطاق العمل", distance);
                    ShowNotification($"🚶 خرجت من نطاق الشركة ({distance:F0} م)");
                }
            }
        }

        #endregion

        #region Location Updates

        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
        {
            var loc = locations.LastOrDefault();
            if (loc == null) return;

            double employeeLat = loc.Coordinate.Latitude;
            double employeeLng = loc.Coordinate.Longitude;
            double distance = Haversine(employeeLat, employeeLng, companyLat, companyLng);

            Console.WriteLine($"📍 Location update: {distance:F0}m from company");

            // ✅ فحص إضافي بجانب Region Monitoring
            if (distance > radiusMeters && !alreadyOutside)
            {
                alreadyOutside = true;
                SendEventInBackgroundAsync("خروج عن نطاق العمل", distance);
                ShowNotification($"⚠️ خارج نطاق الشركة ({distance:F0} م)");
            }
            else if (distance <= radiusMeters && alreadyOutside)
            {
                alreadyOutside = false;
                SendEventInBackgroundAsync("دخول نطاق العمل", distance);
                ShowNotification($"✅ داخل نطاق الشركة ({distance:F0} م)");
            }
        }

        public override void Failed(CLLocationManager manager, NSError error)
        {
            Console.WriteLine($"❌ Location error: {error}");
        }

        #endregion

        #region Notifications

        private void ShowNotification(string message)
        {
            var content = new UNMutableNotificationContent
            {
                Title = "تسجيل الحضور والانصراف",
                Body = message,
                Sound = UNNotificationSound.Default,
                Badge = 1
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            var request = UNNotificationRequest.FromIdentifier(
                Guid.NewGuid().ToString(),
                content,
                trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, err =>
            {
                if (err != null)
                    Console.WriteLine($"⚠️ Notification Error: {err}");
            });
        }

        #endregion

        #region Send Event (with Offline Support)

        private void SendEventInBackgroundAsync(string eventType, double distance)
        {
            // ✅ iOS Background Task
            var taskId = UIKit.UIApplication.SharedApplication.BeginBackgroundTask(() =>
            {
                Console.WriteLine("⏰ Background task expired");
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    // التحقق من الإنترنت
                    bool hasInternet = await HasInternetAsync();

                    if (hasInternet)
                    {
                        // ✅ إرسال مباشر
                        await SendEventToServerAsync(eventType, distance);
                    }
                    else
                    {
                        // ✅ حفظ محلياً
                        await SaveEventOfflineAsync(eventType, distance);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ SendEvent Error: {ex}");

                    // حفظ محلياً في حالة الفشل
                    try
                    {
                        await SaveEventOfflineAsync(eventType, distance);
                    }
                    catch (Exception saveEx)
                    {
                        Console.WriteLine($"❌ Save offline failed: {saveEx}");
                    }
                }
                finally
                {
                    UIKit.UIApplication.SharedApplication.EndBackgroundTask(taskId);
                }
            });
        }

        private async Task SendEventToServerAsync(string eventType, double distance)
        {
            var dto = new { Distance = distance, EventType = eventType, Time = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(dto);

            var token = await SecureStorage.Default.GetAsync("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("⚠️ No access token");
                return;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            string apiUrl = "https://elnagarygroup-001-site1.ktempurl.com/api/LocationApi/exit";

            var response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Event sent: {eventType}");
            }
            else
            {
                Console.WriteLine($"❌ Failed to send event: {response.StatusCode}");
                throw new Exception($"HTTP {response.StatusCode}");
            }
        }

        private async Task SaveEventOfflineAsync(string eventType, double distance)
        {
            var offlineService = new OfflineAttendanceService();
            await offlineService.InitializeAsync();

            await offlineService.SaveLocationEventAsync(new LocalLocationEvent
            {
                EventType = eventType,
                Distance = distance,
                Time = DateTime.UtcNow,
                IsSynced = false
            });

            Console.WriteLine($"💾 Event saved offline: {eventType}");
        }

        private async Task<bool> HasInternetAsync()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.SendAsync(
                    new HttpRequestMessage(
                        HttpMethod.Head,
                        "https://elnagarygroup-001-site1.ktempurl.com"),
                    HttpCompletionOption.ResponseHeadersRead);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // meters
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

        #endregion
    }
}