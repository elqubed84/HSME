//using System;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using HSEM.Models;
//using HSEM.Services;
//using Microsoft.Maui.Storage;

//#if IOS
//namespace HSEM.Platforms.iOS.Services
//{
//    /// <summary>
//    /// تطبيق iOS لخدمة الحضور
//    /// </summary>
//    public class iOSAttendanceService : IPlatformAttendanceService
//    {
//        private const string API_BASE = "https://elnagarygroup-001-site1.ktempurl.com/api";

//        public async Task CheckInAsync(double latitude, double longitude, string ssid, string bssid)
//        {
//            try
//            {
//                // حفظ محلياً أولاً
//                var offlineService = new OfflineAttendanceService();
//                await offlineService.InitializeAsync();

//                var record = new LocalAttendanceRecord
//                {
//                    Action = "IN",
//                    Latitude = latitude,
//                    Longitude = longitude,
//                    SSID = ssid ?? "Unknown",
//                    BSSID = bssid ?? "Unknown",
//                    IsMockLocation = false,
//                    DeviceTime = DateTime.Now
//                };

//                await offlineService.SaveAsync(record);

//                // محاولة الإرسال للسيرفر
//                bool hasInternet = await HasInternetAsync();
//                if (hasInternet)
//                {
//                    bool success = await SendToServerAsync(record);
//                    if (success)
//                    {
//                        await offlineService.DeleteAfterSyncAsync(record.Id);
//                        Console.WriteLine("✅ Check-in sent successfully");
//                    }
//                }
//                else
//                {
//                    Console.WriteLine("📥 Check-in saved offline");
//                }

//                // جدولة Background Sync
//                iOSBackgroundSyncService.ScheduleBackgroundSync();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Check-in error: {ex.Message}");
//                throw;
//            }
//        }

//        public async Task CheckOutAsync(double latitude, double longitude, string ssid, string bssid)
//        {
//            try
//            {
//                // حفظ محلياً أولاً
//                var offlineService = new OfflineAttendanceService();
//                await offlineService.InitializeAsync();

//                var record = new LocalAttendanceRecord
//                {
//                    Action = "OUT",
//                    Latitude = latitude,
//                    Longitude = longitude,
//                    SSID = ssid ?? "Unknown",
//                    BSSID = bssid ?? "Unknown",
//                    IsMockLocation = false,
//                    DeviceTime = DateTime.Now
//                };

//                await offlineService.SaveAsync(record);

//                // محاولة الإرسال للسيرفر
//                bool hasInternet = await HasInternetAsync();
//                if (hasInternet)
//                {
//                    bool success = await SendToServerAsync(record);
//                    if (success)
//                    {
//                        await offlineService.DeleteAfterSyncAsync(record.Id);
//                        Console.WriteLine("✅ Check-out sent successfully");
//                    }
//                }
//                else
//                {
//                    Console.WriteLine("📥 Check-out saved offline");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Check-out error: {ex.Message}");
//                throw;
//            }
//        }

//        public void StartBackgroundTracking(CompanyPrefs prefs)
//        {
//            try
//            {
//                iOSPlatformServices.StartLocationTracking(prefs);
//                iOSBackgroundSyncService.Instance.EnableBackgroundSync();
//                Console.WriteLine("✅ Background tracking started");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Start tracking error: {ex.Message}");
//                throw;
//            }
//        }

//        public void StopBackgroundTracking()
//        {
//            try
//            {
//                iOSPlatformServices.StopLocationTracking();
//                Console.WriteLine("⏹️ Background tracking stopped");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Stop tracking error: {ex.Message}");
//                throw;
//            }
//        }

//        public bool IsTrackingActive()
//        {
//            return iOSPlatformServices.IsLocationTrackingActive();
//        }

//        #region Helper Methods

//        private async Task<bool> SendToServerAsync(LocalAttendanceRecord record)
//        {
//            try
//            {
//                var token = await SecureStorage.Default.GetAsync("AccessToken");
//                if (string.IsNullOrEmpty(token))
//                    return false;

//                using var client = new HttpClient();
//                client.DefaultRequestHeaders.Authorization =
//                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

//                var payload = new
//                {
//                    Latitude = record.Latitude,
//                    Longitude = record.Longitude,
//                    SSID = record.SSID,
//                    BSSID = record.BSSID,
//                    IsMockLocation = record.IsMockLocation,
//                    Action = record.Action,
//                    DeviceTime = record.DeviceTime,
//                    IsOfflineSync = false
//                };

//                var json = JsonSerializer.Serialize(payload);
//                var content = new StringContent(json, Encoding.UTF8, "application/json");

//                var response = await client.PostAsync(
//                    $"{API_BASE}/AttendancePolicy/verify",
//                    content);

//                if (!response.IsSuccessStatusCode)
//                    return false;

//                var body = await response.Content.ReadAsStringAsync();
//                var root = JsonDocument.Parse(body).RootElement;
//                return root.GetProperty("success").GetBoolean();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Send to server error: {ex.Message}");
//                return false;
//            }
//        }

//        private async Task<bool> HasInternetAsync()
//        {
//            try
//            {
//                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
//                var response = await client.SendAsync(
//                    new HttpRequestMessage(HttpMethod.Head, API_BASE),
//                    HttpCompletionOption.ResponseHeadersRead);

//                return response.IsSuccessStatusCode;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        #endregion
//    }
//}
//#endif