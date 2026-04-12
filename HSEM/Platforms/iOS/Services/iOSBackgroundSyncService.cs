using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BackgroundTasks;
using Foundation;
using HSEM.Models;
using HSEM.Services;
using Microsoft.Maui.Storage;

namespace HSEM.Platforms.iOS.Services
{
    /// <summary>
    /// خدمة المزامنة الخلفية لـ iOS
    /// تستخدم BGTaskScheduler لمزامنة البيانات المحفوظة محلياً
    /// </summary>
    public class iOSBackgroundSyncService
    {
        private const string SYNC_TASK_ID = "com.extrateam.elnagary.sync";
        private const string LOCATION_REFRESH_ID = "com.extrateam.elnagary.location-refresh";

        private static iOSBackgroundSyncService _instance;
        public static iOSBackgroundSyncService Instance => _instance ??= new iOSBackgroundSyncService();

        private iOSBackgroundSyncService() { }

        #region Task Registration

        /// <summary>
        /// تسجيل Background Tasks في AppDelegate
        /// </summary>
        public void RegisterBackgroundTasks()
        {
            // Background Sync Task
            BGTaskScheduler.Shared.Register(SYNC_TASK_ID, null, task =>
            {
                HandleBackgroundSync(task as BGProcessingTask);
            });

            // Location Refresh Task
            BGTaskScheduler.Shared.Register(LOCATION_REFRESH_ID, null, task =>
            {
                HandleLocationRefresh(task as BGAppRefreshTask);
            });

            Console.WriteLine("✅ iOS Background Tasks registered");
        }

        #endregion

        #region Task Scheduling

        /// <summary>
        /// جدولة مهمة المزامنة التالية
        /// </summary>
        public static void ScheduleBackgroundSync()
        {
            var request = new BGProcessingTaskRequest(SYNC_TASK_ID)
            {
                RequiresNetworkConnectivity = true,
                RequiresExternalPower = false
            };

            try
            {
                BGTaskScheduler.Shared.Submit(request, out var error);
                if (error != null)
                {
                    Console.WriteLine($"⚠️ Failed to schedule sync: {error.LocalizedDescription}");
                }
                else
                {
                    Console.WriteLine("✅ Background sync scheduled");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Schedule error: {ex.Message}");
            }
        }

        /// <summary>
        /// جدولة مهمة تحديث الموقع
        /// </summary>
        public static void ScheduleLocationRefresh()
        {
            var request = new BGAppRefreshTaskRequest(LOCATION_REFRESH_ID)
            {
                EarliestBeginDate = NSDate.FromTimeIntervalSinceNow(15 * 60) // بعد 15 دقيقة
            };

            try
            {
                BGTaskScheduler.Shared.Submit(request, out var error);
                if (error != null)
                {
                    Console.WriteLine($"⚠️ Failed to schedule location refresh: {error.LocalizedDescription}");
                }
                else
                {
                    Console.WriteLine("✅ Location refresh scheduled");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Schedule error: {ex.Message}");
            }
        }

        #endregion

        #region Task Handlers

        /// <summary>
        /// معالج مهمة المزامنة الخلفية
        /// </summary>
        private async void HandleBackgroundSync(BGProcessingTask task)
        {
            Console.WriteLine("🔄 Background sync task started");

            // جدولة المهمة التالية
            ScheduleBackgroundSync();

            // إنشاء Cancellation Token
            var cts = new System.Threading.CancellationTokenSource();

            // معالج انتهاء الوقت
            task.ExpirationHandler = () =>
            {
                cts.Cancel();
                Console.WriteLine("⏰ Background task expired");
                task.SetTaskCompleted(false);
            };

            try
            {
                // تنفيذ المزامنة
                bool success = await PerformSyncAsync(cts.Token);
                task.SetTaskCompleted(success);

                if (success)
                {
                    Console.WriteLine("✅ Background sync completed successfully");
                }
                else
                {
                    Console.WriteLine("⚠️ Background sync completed with errors");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Background sync failed: {ex.Message}");
                task.SetTaskCompleted(false);
            }
        }

        /// <summary>
        /// معالج مهمة تحديث الموقع
        /// </summary>
        private void HandleLocationRefresh(BGAppRefreshTask task)
        {
            Console.WriteLine("📍 Location refresh task started");

            // جدولة المهمة التالية
            ScheduleLocationRefresh();

            // تفعيل تحديث الموقع
            // LocationTrackingService سيقوم بالتحديث تلقائياً

            task.SetTaskCompleted(true);
            Console.WriteLine("✅ Location refresh completed");
        }

        #endregion

        #region Sync Logic

        /// <summary>
        /// تنفيذ المزامنة الفعلية
        /// </summary>
        private async Task<bool> PerformSyncAsync(System.Threading.CancellationToken cancellationToken)
        {
            // 1. التحقق من الاتصال بالإنترنت
            bool hasInternet = await HasInternetAsync();
            if (!hasInternet)
            {
                Console.WriteLine("❌ No internet connection");
                return false;
            }

            var offlineService = new OfflineAttendanceService();
            await offlineService.InitializeAsync();

            bool allSuccess = true;

            // 2. مزامنة سجلات الحضور/الانصراف
            var pendingAttendance = await offlineService.GetPendingAsync();
            Console.WriteLine($"📋 Found {pendingAttendance.Count} pending attendance records");

            foreach (var record in pendingAttendance)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("⏹️ Sync cancelled");
                    break;
                }

                try
                {
                    bool success = await SyncAttendanceRecord(record);
                    if (success)
                    {
                        await offlineService.DeleteAfterSyncAsync(record.Id);
                        Console.WriteLine($"✅ Synced attendance: {record.Action} at {record.DeviceTime:HH:mm}");
                    }
                    else
                    {
                        allSuccess = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to sync attendance: {ex.Message}");
                    allSuccess = false;
                }
            }

            // 3. مزامنة أحداث الموقع
            var pendingEvents = await offlineService.GetPendingLocationEventsAsync();
            Console.WriteLine($"📍 Found {pendingEvents.Count} pending location events");

            foreach (var evt in pendingEvents)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    bool success = await SyncLocationEvent(evt);
                    if (success)
                    {
                        await offlineService.DeleteLocationEventAsync(evt.Id);
                        Console.WriteLine($"✅ Synced location event: {evt.EventType}");
                    }
                    else
                    {
                        allSuccess = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to sync location event: {ex.Message}");
                    allSuccess = false;
                }
            }

            return allSuccess;
        }

        /// <summary>
        /// مزامنة سجل حضور واحد
        /// </summary>
        private async Task<bool> SyncAttendanceRecord(LocalAttendanceRecord record)
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("⚠️ No access token available");
                    return false;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var payload = new
                {
                    Latitude = record.Latitude,
                    Longitude = record.Longitude,
                    SSID = record.SSID,
                    BSSID = record.BSSID,
                    IsMockLocation = record.IsMockLocation,
                    Action = record.Action,
                    DeviceTime = record.DeviceTime,
                    IsOfflineSync = true
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/AttendancePolicy/verify",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"⚠️ Server returned: {response.StatusCode}");
                    return false;
                }

                var body = await response.Content.ReadAsStringAsync();
                var root = JsonDocument.Parse(body).RootElement;
                bool success = root.GetProperty("success").GetBoolean();

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Sync attendance error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// مزامنة حدث موقع واحد
        /// </summary>
        private async Task<bool> SyncLocationEvent(LocalLocationEvent evt)
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (string.IsNullOrEmpty(token))
                    return false;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var payload = new
                {
                    Distance = evt.Distance,
                    EventType = evt.EventType,
                    Time = evt.Time
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/LocationApi/exit",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Sync location event error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// التحقق من الاتصال بالإنترنت
        /// </summary>
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

        #region Public Methods

        /// <summary>
        /// تفعيل المزامنة الخلفية
        /// </summary>
        public void EnableBackgroundSync()
        {
            ScheduleBackgroundSync();
            ScheduleLocationRefresh();
            Console.WriteLine("✅ Background sync enabled");
        }

        /// <summary>
        /// إيقاف المزامنة الخلفية
        /// </summary>
        public void DisableBackgroundSync()
        {
            BGTaskScheduler.Shared.Cancel(SYNC_TASK_ID);
            BGTaskScheduler.Shared.Cancel(LOCATION_REFRESH_ID);
            Console.WriteLine("⏹️ Background sync disabled");
        }

        /// <summary>
        /// تنفيذ مزامنة فورية (من الـ foreground)
        /// </summary>
        public async Task<bool> SyncNowAsync()
        {
            Console.WriteLine("🔄 Manual sync started");
            var cts = new System.Threading.CancellationTokenSource();
            return await PerformSyncAsync(cts.Token);
        }

        #endregion
    }
}