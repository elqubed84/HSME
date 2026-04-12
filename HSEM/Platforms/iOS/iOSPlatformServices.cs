using System;
using HSEM.Platforms.iOS.Services;
using HSEM.Models;
using HSEM.Services;

namespace HSEM.Platforms.iOS
{
    /// <summary>
    /// خدمة تهيئة منصة iOS
    /// تستدعى من MauiProgram أو من LoginPage بعد تسجيل الدخول
    /// </summary>
    public static class iOSPlatformServices
    {
        private static LocationTrackingService _locationService;
        private static bool _isInitialized = false;

        /// <summary>
        /// تهيئة خدمات iOS
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Console.WriteLine("⚠️ iOS services already initialized");
                return;
            }

            try
            {
                // 1. تسجيل Background Tasks
                iOSBackgroundSyncService.Instance.RegisterBackgroundTasks();

                // 2. تفعيل Background Sync
                iOSBackgroundSyncService.Instance.EnableBackgroundSync();

                _isInitialized = true;
                Console.WriteLine("✅ iOS Platform Services initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ iOS initialization error: {ex.Message}");
            }
        }

        /// <summary>
        /// بدء تتبع الموقع
        /// يُستدعى بعد تسجيل الحضور
        /// </summary>
        public static void StartLocationTracking(CompanyPrefs prefs)
        {
            try
            {
                if (_locationService != null)
                {
                    Console.WriteLine("⚠️ Location tracking already running");
                    return;
                }

                _locationService = new LocationTrackingService(
                    prefs.CompanyLat,
                    prefs.CompanyLng,
                    prefs.RadiusMeters);

                _locationService.StartTracking();

                Console.WriteLine("✅ Location tracking started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Location tracking error: {ex.Message}");
            }
        }

        /// <summary>
        /// إيقاف تتبع الموقع
        /// يُستدعى بعد تسجيل الانصراف
        /// </summary>
        public static void StopLocationTracking()
        {
            try
            {
                if (_locationService == null)
                {
                    Console.WriteLine("⚠️ Location tracking not running");
                    return;
                }

                _locationService.StopTracking();
                _locationService = null;

                Console.WriteLine("⏹️ Location tracking stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Stop tracking error: {ex.Message}");
            }
        }

        /// <summary>
        /// تنفيذ مزامنة فورية
        /// </summary>
        public static async System.Threading.Tasks.Task<bool> SyncNowAsync()
        {
            try
            {
                return await iOSBackgroundSyncService.Instance.SyncNowAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Sync error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// التحقق من حالة الخدمات
        /// </summary>
        public static bool IsLocationTrackingActive()
        {
            return _locationService != null;
        }

        /// <summary>
        /// إعادة تشغيل الخدمات بعد إعادة فتح التطبيق
        /// </summary>
        public static void Resume()
        {
            if (_isInitialized)
            {
                // إعادة جدولة Background Tasks
                iOSBackgroundSyncService.ScheduleBackgroundSync();
                iOSBackgroundSyncService.ScheduleLocationRefresh();

                Console.WriteLine("✅ iOS services resumed");
            }
        }

        /// <summary>
        /// تنظيف الموارد عند إيقاف التطبيق
        /// </summary>
        public static void Cleanup()
        {
            try
            {
                StopLocationTracking();
                iOSBackgroundSyncService.Instance.DisableBackgroundSync();

                _isInitialized = false;
                Console.WriteLine("✅ iOS services cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Cleanup error: {ex.Message}");
            }
        }
    }
}