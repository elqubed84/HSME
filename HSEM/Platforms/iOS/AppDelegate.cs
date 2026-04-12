using BackgroundTasks;
using CoreLocation;
using Firebase.CloudMessaging;
using Firebase.Core;
using Foundation;
using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;

namespace HSEM.Platforms.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        private const string SYNC_TASK_ID = "com.extrateam.hsem.sync";
        private const string LOCATION_REFRESH_ID = "com.extrateam.hsem.location-refresh";

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // ===== 1. Firebase Configuration =====
            try
            {
                App.Configure();
                Console.WriteLine("✅ Firebase configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Firebase configuration failed: {ex.Message}");
            }

            // ===== 2. Push Notifications Setup =====
            SetupPushNotifications(application);

            // ===== 3. Background Tasks Registration =====
            RegisterBackgroundTasks();

            // ===== 4. Location Manager Setup =====
            SetupLocationManager();

            return base.FinishedLaunching(application, launchOptions);
        }

        #region Push Notifications

        private void SetupPushNotifications(UIApplication application)
        {
            // Request notification permissions
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                (granted, error) =>
                {
                    if (granted)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            application.RegisterForRemoteNotifications();
                        });
                    }
                });

            // Set notification delegate
            UNUserNotificationCenter.Current.Delegate = new iOSNotificationDelegate();

            // FCM Token refresh
            Messaging.SharedInstance.AutoInitEnabled = true;
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            // Set APNs token for FCM
            Messaging.SharedInstance.ApnsToken = deviceToken;
            Console.WriteLine("✅ APNs token set for FCM");
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            Console.WriteLine($"❌ Failed to register for remote notifications: {error}");
        }

        public override void DidReceiveRemoteNotification(
            UIApplication application,
            NSDictionary userInfo,
            Action<UIBackgroundFetchResult> completionHandler)
        {
            // Handle remote notification
            Console.WriteLine("📩 Remote notification received");

            // Process notification
            if (userInfo.ContainsKey(new NSString("aps")))
            {
                // Handle the notification
                completionHandler(UIBackgroundFetchResult.NewData);
            }
            else
            {
                completionHandler(UIBackgroundFetchResult.NoData);
            }
        }

        #endregion

        #region Background Tasks

        private void RegisterBackgroundTasks()
        {
            // Register background sync task
            BGTaskScheduler.Shared.Register(SYNC_TASK_ID, null, task =>
            {
                HandleBackgroundSync(task as BGProcessingTask);
            });

            // Register location refresh task
            BGTaskScheduler.Shared.Register(LOCATION_REFRESH_ID, null, task =>
            {
                HandleLocationRefresh(task as BGAppRefreshTask);
            });

            Console.WriteLine("✅ Background tasks registered");
        }

        private async void HandleBackgroundSync(BGProcessingTask task)
        {
            Console.WriteLine("🔄 Background sync task started");

            // Schedule next task
            ScheduleBackgroundSync();

            // Create cancellation token
            var cts = new CancellationTokenSource();
            task.ExpirationHandler = () =>
            {
                cts.Cancel();
                Console.WriteLine("⏰ Background task expired");
            };

            try
            {
                // Perform sync
                var syncService = new HSEM.Services.OfflineAttendanceService();
                await syncService.InitializeAsync();

                // Check internet
                bool hasInternet = await HasInternetAsync();
                if (!hasInternet)
                {
                    task.SetTaskCompleted(false);
                    return;
                }

                // Sync pending attendance
                var pending = await syncService.GetPendingAsync();
                foreach (var record in pending)
                {
                    if (cts.Token.IsCancellationRequested) break;

                    try
                    {
                        // Send to server
                        var success = await SendAttendanceToServer(record);
                        if (success)
                        {
                            await syncService.DeleteAfterSyncAsync(record.Id);
                            Console.WriteLine($"✅ Synced: {record.Action} at {record.DeviceTime}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Sync error: {ex.Message}");
                    }
                }

                task.SetTaskCompleted(true);
                Console.WriteLine("✅ Background sync completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Background sync failed: {ex.Message}");
                task.SetTaskCompleted(false);
            }
        }

        private void HandleLocationRefresh(BGAppRefreshTask task)
        {
            Console.WriteLine("📍 Location refresh task started");

            // Schedule next refresh
            ScheduleLocationRefresh();

            // Trigger location update
            var locationService = new HSEM.Platforms.iOS.Services.LocationTrackingService(0, 0, 0);
            // Location manager will trigger updates

            task.SetTaskCompleted(true);
        }

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
                    Console.WriteLine($"⚠️ Failed to schedule sync: {error}");
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

        public static void ScheduleLocationRefresh()
        {
            var request = new BGAppRefreshTaskRequest(LOCATION_REFRESH_ID)
            {
                EarliestBeginDate = NSDate.FromTimeIntervalSinceNow(15 * 60) // 15 minutes
            };

            try
            {
                BGTaskScheduler.Shared.Submit(request, out var error);
                if (error != null)
                {
                    Console.WriteLine($"⚠️ Failed to schedule location refresh: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Schedule error: {ex.Message}");
            }
        }

        #endregion

        #region Location Manager

        private void SetupLocationManager()
        {
            // Request location permissions
            var locationManager = new CLLocationManager
            {
                AllowsBackgroundLocationUpdates = true,
                PausesLocationUpdatesAutomatically = false
            };

            locationManager.RequestAlwaysAuthorization();
            Console.WriteLine("✅ Location permissions requested");
        }

        #endregion

        #region Helper Methods

        private async Task<bool> HasInternetAsync()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                var response = await client.SendAsync(
                    new System.Net.Http.HttpRequestMessage(
                        System.Net.Http.HttpMethod.Head,
                        "https://elnagarygroup-001-site1.ktempurl.com"),
                    System.Net.Http.HttpCompletionOption.ResponseHeadersRead);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendAttendanceToServer(HSEM.Models.LocalAttendanceRecord record)
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (string.IsNullOrEmpty(token))
                    return false;

                using var client = new System.Net.Http.HttpClient();
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

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new System.Net.Http.StringContent(
                    json,
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/AttendancePolicy/verify",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Send error: {ex.Message}");
                return false;
            }
        }

        #endregion
    }

    #region iOS Notification Delegate

    public class iOSNotificationDelegate : UNUserNotificationCenterDelegate
    {
        public override void WillPresentNotification(
            UNUserNotificationCenter center,
            UNNotification notification,
            Action<UNNotificationPresentationOptions> completionHandler)
        {
            // Show notification even when app is in foreground
            completionHandler(UNNotificationPresentationOptions.Alert |
                            UNNotificationPresentationOptions.Sound |
                            UNNotificationPresentationOptions.Badge);
        }

        public override void DidReceiveNotificationResponse(
            UNUserNotificationCenter center,
            UNNotificationResponse response,
            Action completionHandler)
        {
            // Handle notification tap
            Console.WriteLine("📱 Notification tapped");
            completionHandler();
        }
    }

    #endregion
}