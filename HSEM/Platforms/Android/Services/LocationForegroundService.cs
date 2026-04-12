using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using HSEM.Models;
using HSEM.Services;
using HSEM.Views;
using System.Text;
using System.Text.Json;

namespace HSEM.Platforms.Android.Services
{
    [Service(Exported = false, ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
    public class LocationForegroundService : Service
    {
        const string CHANNEL_ID = "yourapp_location_channel";
        const int NOTIF_ID = 101;
        const string ACTION_START = "ACTION_START_LOCATION_SERVICE"; // حضور + تتبع
        const string ACTION_STOP = "ACTION_STOP_LOCATION_SERVICE";  // انصراف + وقف تتبع
        const string ACTION_START_SYNC = "ACTION_START_SYNC_ONLY";        // sync بدون تتبع

        IFusedLocationProviderClient _fusedClient;
        LocationCallbackImpl _locationCallback;
        LocationRequest _locationRequest;
        CompanyPrefs _prefs;
        CancellationTokenSource _syncCts;

        // =============================================
        //  OnCreate
        // =============================================
        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannel();

            _prefs = PreferencesHelper.LoadCompanyPrefs();
            if (_prefs == null) return;

            _fusedClient = LocationServices.GetFusedLocationProviderClient(
                global::Android.App.Application.Context);

            _locationRequest = LocationRequest.Create();
            _locationRequest.SetInterval(1 * 60 * 1000);
            _locationRequest.SetFastestInterval(2 * 60 * 1000);
            _locationRequest.SetPriority(LocationRequest.PriorityLowPower);

            _locationCallback = new LocationCallbackImpl(this, _prefs);
        }

        public override IBinder OnBind(Intent intent) => null;

        // =============================================
        //  OnStartCommand
        // =============================================
        public override StartCommandResult OnStartCommand(
            Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                var action = intent?.Action;

                if (action == ACTION_START_SYNC)
                {
                    // ✅ sync فقط بدون تتبع موقع
                    StartForeground(NOTIF_ID, BuildNotification("📱 أنت نشط الأن"));

                    _syncCts?.Cancel();
                    _syncCts = new CancellationTokenSource();
                    _ = Task.Run(() => BackgroundSyncLoopAsync(_syncCts.Token));
                }
                else if (action == ACTION_START)
                {
                    // ✅ حضور فعلي — تتبع + sync
                    StartForeground(NOTIF_ID, BuildNotification("تم تسجيل حضورك ✅"));

                    if (_fusedClient != null && _locationCallback != null)
                        RequestLocationUpdates();

                    _syncCts?.Cancel();
                    _syncCts = new CancellationTokenSource();
                    _ = Task.Run(() => BackgroundSyncLoopAsync(_syncCts.Token));
                }
                else if (action == ACTION_STOP)
                {
                    // ✅ انصراف — وقف التتبع
                    _syncCts?.Cancel();
                    _syncCts?.Dispose();
                    _syncCts = null;

                    if (_fusedClient != null && _locationCallback != null)
                        _fusedClient.RemoveLocationUpdates(_locationCallback);

                    StopForeground(true);
                    StopSelf();
                }
            }
            catch (Exception ex)
            {
                Log.Error("LocationService", ex.ToString());
            }

            return StartCommandResult.Sticky;
        }

        // =============================================
        //  Background Sync Loop
        //  بيبعت حضور/انصراف + أحداث الموقع
        // =============================================
        private async Task BackgroundSyncLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(2), token);

                    var offlineService = new OfflineAttendanceService();
                    await offlineService.InitializeAsync();

                    // ===== فحص النت =====
                    bool hasNet = await HasRealInternetAsync();
                    if (!hasNet) continue;

                    // ===== 1. بعت سجلات الحضور/الانصراف =====
                    var pendingAttendance = await offlineService.GetPendingAsync();
                    foreach (var record in pendingAttendance)
                    {
                        if (token.IsCancellationRequested) break;
                        try
                        {
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

                            var response = await LoginPage.PostWithTokenAsync(
                                "https://elnagarygroup-001-site1.ktempurl.com/api/AttendancePolicy/verify",
                                new StringContent(
                                    JsonSerializer.Serialize(payload),
                                    Encoding.UTF8, "application/json"));

                            if (!response.IsSuccessStatusCode) continue;

                            var body = await response.Content.ReadAsStringAsync();
                            var root = JsonDocument.Parse(body).RootElement;
                            bool success = root.GetProperty("success").GetBoolean();

                            if (success)
                            {
                                await offlineService.DeleteAfterSyncAsync(record.Id);
                                Log.Debug("BackgroundSync",
                                    $"✅ {record.Action} {record.DeviceTime:hh:mm tt} synced");
                                UpdateNotification(record.Action == "IN"
                                    ? "تم تسجيل حضورك بنجاح ✅"
                                    : "تم تسجيل انصرافك بنجاح ✅");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("BackgroundSync", $"Attendance error: {ex.Message}");
                        }
                    }

                    // ===== 2. بعت أحداث الموقع =====
                    var pendingEvents = await offlineService.GetPendingLocationEventsAsync();
                    foreach (var evt in pendingEvents)
                    {
                        if (token.IsCancellationRequested) break;
                        try
                        {
                            var json = JsonSerializer.Serialize(new
                            {
                                Distance = evt.Distance,
                                EventType = evt.EventType,
                                Time = evt.Time
                            });

                            var token2 = await SecureStorage.Default.GetAsync("AccessToken");
                            if (string.IsNullOrEmpty(token2)) continue;

                            using var client = new System.Net.Http.HttpClient();
                            client.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

                            var response = await client.PostAsync(
                                "https://elnagarygroup-001-site1.ktempurl.com/api/LocationApi/exit",
                                new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json"));

                            if (response.IsSuccessStatusCode)
                            {
                                await offlineService.DeleteLocationEventAsync(evt.Id);
                                Log.Debug("BackgroundSync", $"✅ Location event synced: {evt.EventType}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("BackgroundSync", $"Location event error: {ex.Message}");
                        }
                    }
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    Log.Error("BackgroundSync", ex.ToString());
                }
            }
        }

        // =============================================
        //  التحقق من نت حقيقي
        // =============================================
        private static async Task<bool> HasRealInternetAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return false;
            try
            {
                using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.SendAsync(
                    new System.Net.Http.HttpRequestMessage(
                        System.Net.Http.HttpMethod.Head,
                        "https://elnagarygroup-001-site1.ktempurl.com"),
                    System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // =============================================
        //  Location Updates
        // =============================================
        void RequestLocationUpdates()
        {
            try
            {
                _fusedClient.RequestLocationUpdates(
                    _locationRequest, _locationCallback, Looper.MainLooper);
            }
            catch (Java.Lang.SecurityException ex)
            {
                Log.Error("LocationService", "Missing permission: " + ex);
            }
            catch (Exception ex)
            {
                Log.Error("LocationService", ex.ToString());
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            try
            {
                _syncCts?.Cancel();
                _syncCts?.Dispose();
                _fusedClient?.RemoveLocationUpdates(_locationCallback);
            }
            catch { }
        }

        // =============================================
        //  Notification
        // =============================================
        Notification BuildNotification(string content)
        {
            var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("تسجيل الحضور والانصراف")
                .SetContentText(content)
                .SetSmallIcon(Resource.Drawable.abeer)
                .SetOngoing(true)
                .SetPriority(NotificationCompat.PriorityLow);

            var i = new Intent(this, typeof(MainActivity));
            i.AddFlags(ActivityFlags.SingleTop);
            builder.SetContentIntent(
                PendingIntent.GetActivity(this, 0, i, PendingIntentFlags.Immutable));

            return builder.Build();
        }

        void UpdateNotification(string content)
        {
            try
            {
                ((NotificationManager)GetSystemService(NotificationService))
                    ?.Notify(NOTIF_ID, BuildNotification(content));
            }
            catch { }
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var chan = new NotificationChannel(
                    CHANNEL_ID, "Location Service", NotificationImportance.Default)
                {
                    Description = "Location tracking while working"
                };
                ((NotificationManager)GetSystemService(NotificationService))
                    .CreateNotificationChannel(chan);
            }
        }

        // =============================================
        //  LocationCallbackImpl
        //  ✅ لو مفيش نت → يحفظ محلياً
        //  ✅ لو في نت → يبعت مباشرة
        // =============================================
        class LocationCallbackImpl : LocationCallback
        {
            readonly LocationForegroundService _svc;
            readonly CompanyPrefs _prefs;
            static bool alreadyOutside = false;

            public LocationCallbackImpl(LocationForegroundService svc, CompanyPrefs prefs)
            {
                _svc = svc;
                _prefs = prefs;
            }

            public override void OnLocationResult(LocationResult result)
            {
                base.OnLocationResult(result);
                var loc = result?.LastLocation;
                if (loc == null) return;

                try
                {
                    double distance = Haversine(
                        loc.Latitude, loc.Longitude,
                        _prefs.CompanyLat, _prefs.CompanyLng);

                    if (distance > _prefs.RadiusMeters + 20 && !alreadyOutside)
                    {
                        alreadyOutside = true;
                        Task.Run(() => SendOrSaveEventAsync("خروج عن نطاق العمل", distance));
                    }
                    else if (distance <= _prefs.RadiusMeters && alreadyOutside)
                    {
                        alreadyOutside = false;
                        Task.Run(() => SendOrSaveEventAsync("دخول نطاق العمل", distance));
                    }
                    else if (distance <= _prefs.RadiusMeters)
                    {
                        alreadyOutside = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("LocationCallback", ex.ToString());
                }
            }

            // ✅ يحاول يبعت — لو فشل يحفظ محلياً
            static async Task SendOrSaveEventAsync(string eventType, double distance)
            {
                bool hasNet = false;
                try
                {
                    using var testClient = new System.Net.Http.HttpClient
                    { Timeout = TimeSpan.FromSeconds(5) };
                    var testResp = await testClient.SendAsync(
                        new System.Net.Http.HttpRequestMessage(
                            System.Net.Http.HttpMethod.Head,
                            "https://elnagarygroup-001-site1.ktempurl.com"),
                        System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                    hasNet = testResp.IsSuccessStatusCode;
                }
                catch { hasNet = false; }

                if (hasNet)
                {
                    // ✅ في نت → ابعت مباشرة
                    await SendEventToServerAsync(eventType, distance);
                }
                else
                {
                    // ✅ مفيش نت → احفظ محلياً
                    try
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
                        Log.Debug("LocationCallback",
                            $"📥 Location event saved offline: {eventType}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error("LocationCallback", $"Save offline error: {ex.Message}");
                    }
                }
            }

            static async Task SendEventToServerAsync(string eventType, double distance)
            {
                try
                {
                    var json = JsonSerializer.Serialize(new
                    {
                        Distance = distance,
                        EventType = eventType,
                        Time = DateTime.UtcNow
                    });

                    var token = await SecureStorage.Default.GetAsync("AccessToken");
                    if (string.IsNullOrEmpty(token)) return;

                    using var client = new System.Net.Http.HttpClient();
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.PostAsync(
                        "https://elnagarygroup-001-site1.ktempurl.com/api/LocationApi/exit",
                        new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json"));

                    if (!response.IsSuccessStatusCode)
                        Log.Error("LocationService", $"فشل: {response.StatusCode}");
                    else
                        Log.Debug("LocationCallback", $"✅ Event sent: {eventType}");
                }
                catch (Exception ex)
                {
                    Log.Error("LocationCallback", ex.ToString());
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
                return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            }
        }
    }
}