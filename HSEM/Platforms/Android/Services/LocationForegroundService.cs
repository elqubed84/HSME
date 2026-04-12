using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using HSEM.Models;
using HSEM.Services;
using HSEM.Views;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HSEM.Platforms.Android.Services
{
    [Service(Exported = false, ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LocationForegroundService : Service
    {
        const string CHANNEL_ID = "hsem_location_channel";
        const int NOTIF_ID = 101;

        const string ACTION_START = "ACTION_START_LOCATION_SERVICE";
        const string ACTION_STOP = "ACTION_STOP_LOCATION_SERVICE";
        const string ACTION_START_SYNC = "ACTION_START_SYNC_ONLY";

        CompanyPrefs _prefs;
        CancellationTokenSource _cts;
        bool _running;

        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannel();

            _prefs = PreferencesHelper.LoadCompanyPrefs();
        }

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var action = intent?.Action;

            if (action == ACTION_START)
            {
                StartService();
            }
            else if (action == ACTION_STOP)
            {
                StopService();
            }
            else if (action == ACTION_START_SYNC)
            {
                StartService(syncOnly: true);
            }

            return StartCommandResult.Sticky;
        }

        void StartService(bool syncOnly = false)
        {
            if (_running) return;

            _cts = new CancellationTokenSource();
            _running = true;

            StartForeground(NOTIF_ID, BuildNotification("الخدمة تعمل"));

            Task.Run(() => TrackingLoop(_cts.Token));
            Task.Run(() => SyncLoop(_cts.Token));
        }

        void StopService()
        {
            _cts?.Cancel();
            _running = false;

            StopForeground(true);
            StopSelf();
        }

        // =========================
        // TRACKING LOOP
        // =========================
        async Task TrackingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var request = new GeolocationRequest(
    GeolocationAccuracy.Medium,
    TimeSpan.FromSeconds(10));

                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                        ProcessLocation(location.Latitude, location.Longitude);
                }
                catch (Exception ex)
                {
                    Log.Error("TrackingLoop", ex.Message);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), token);
            }
        }

        // =========================
        // LOCATION LOGIC
        // =========================
        void ProcessLocation(double lat, double lng)
        {
            double distance = Haversine(
                lat, lng,
                _prefs.CompanyLat,
                _prefs.CompanyLng);

            if (distance > _prefs.RadiusMeters + 20)
                QueueEvent("OUT", distance);
            else
                QueueEvent("IN", distance);
        }

        // =========================
        // EVENT QUEUE
        // =========================
        async void QueueEvent(string type, double distance)
        {
            var evt = new LocalLocationEvent
            {
                EventType = type,
                Distance = distance,
                Time = DateTime.UtcNow,
                IsSynced = false
            };

            if (!await HasInternet())
            {
                var offline = new OfflineAttendanceService();
                await offline.InitializeAsync();
                await offline.SaveLocationEventAsync(evt);
                return;
            }

            await SendToServer(evt);
        }

        // =========================
        // SYNC LOOP
        // =========================
        async Task SyncLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(3), token);

                    if (!await HasInternet())
                        continue;

                    var offline = new OfflineAttendanceService();
                    await offline.InitializeAsync();

                    var items = await offline.GetPendingLocationEventsAsync();

                    foreach (var item in items)
                    {
                        await SendToServer(item);
                        await offline.DeleteLocationEventAsync(item.Id);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("SyncLoop", ex.Message);
                }
            }
        }

        // =========================
        // SEND TO SERVER
        // =========================
        async Task SendToServer(LocalLocationEvent evt)
        {
            try
            {
                var json = JsonSerializer.Serialize(new
                {
                    evt.Distance,
                    evt.EventType,
                    evt.Time
                });

                var token = await SecureStorage.Default.GetAsync("AccessToken");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                await client.PostAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/LocationApi/exit",
                    new StringContent(json, Encoding.UTF8, "application/json"));
            }
            catch (Exception ex)
            {
                Log.Error("SendToServer", ex.Message);
            }
        }

        // =========================
        // INTERNET CHECK
        // =========================
        async Task<bool> HasInternet()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return false;

            try
            {
                using var client = new HttpClient();
                var res = await client.GetAsync("https://elnagarygroup-001-site1.ktempurl.com");
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // =========================
        // NOTIFICATION
        // =========================
        Notification BuildNotification(string text)
        {
            var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("HSEM Location Service")
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.abeer)
                .SetOngoing(true);

            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);

            builder.SetContentIntent(
                PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable));

            return builder.Build();
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Location Service",
                    NotificationImportance.Default);

                var manager = (NotificationManager)GetSystemService(NotificationService);
                manager.CreateNotificationChannel(channel);
            }
        }

        // =========================
        // HAVERSINE
        // =========================
        double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3;

            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _cts?.Cancel();
        }
    }
}