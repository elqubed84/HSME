using HSEM.ViewModels;
using System.Text;
using System.Text.Json;
using HSEM.Services;
using HSEM.Interfaces;
#if ANDROID
using Android.Content;
using Android.App;
using HSEM.Platforms.Android.Services;
using Android.Provider;
using HSEM.Helper;
#endif
#if IOS
using CoreLocation;
using Foundation;
using UIKit;
#endif
using Microsoft.Maui.ApplicationModel;

namespace HSEM.Views;

public partial class Dashboard : ContentPage
{
    private readonly IPopupService _alertService;
#if IOS
    private LocationTrackingService _locationService;
#endif

    public Dashboard()
    {
        InitializeComponent();
        _alertService = new PopupService();
        BindingContext = new DashboardViewModel();
    }

    // =============================================
    //  Lifecycle
    // =============================================
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // ✅ التحقق من وجود فرع معين
        CheckBranchAssignment();

        bool granted = await EnsureLocationAlwaysPermissionAsync();
        if (!granted)
            await Navigation.PopAsync();
    }

    // =============================================
    // ✅ جديد: التحقق من الفرع المعين
    // =============================================
   private void CheckBranchAssignment()
{
    bool hasBranch = Preferences.ContainsKey("BranchId");
    bool isAdmin = Preferences.Get("UserRole", false);

    if (!hasBranch && !isAdmin)
    {
        // ✅ موظف بدون فرع
        AttendanceButton.IsEnabled = false;
        AttendanceExitButton.IsEnabled = false;
        BranchInfoFrame.IsVisible = false;
        WarningFrame.IsVisible = true;
    }
    else if (hasBranch)
    {
        // ✅ موظف معاه فرع
        string branchName = Preferences.Get("BranchName", "غير محدد");
        bool requireGps = Preferences.Get("RequireGps", true);
        bool requireWifi = Preferences.Get("RequireWifi", false);

        BranchLabel.Text = branchName;
        RequireGpsLabel.Text = requireGps ? "📡 GPS: مطلوب" : "📡 GPS: غير مطلوب";
        RequireWifiLabel.Text = requireWifi ? "📶 WiFi: مطلوب" : "📶 WiFi: غير مطلوب";

        BranchInfoFrame.IsVisible = true;
        WarningFrame.IsVisible = false;
        AttendanceButton.IsVisible = true;
        AttendanceExitButton.IsVisible = true;
    }
    else if (isAdmin)
    {
        // ✅ Admin/HR
        BranchLabel.Text = "👑 حساب إداري";
        RequireGpsLabel.IsVisible = false;
        RequireWifiLabel.IsVisible = false;

        BranchInfoFrame.IsVisible = true;
        WarningFrame.IsVisible = false;
        AttendanceButton.IsVisible = true;
        AttendanceExitButton.IsVisible = true;
    }
}

    private async Task<bool> EnsureLocationAlwaysPermissionAsync()
    {
        while (true)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied)
            {
                var result = await Permissions.RequestAsync<Permissions.LocationAlways>();
                if (result == PermissionStatus.Granted)
                    return true;
            }

            bool openSettings = await DisplayAlert(
                "إذن مطلوب ⚠️",
                "يجب منح إذن الموقع «السماح دائماً» لاستخدام التطبيق.\n\nافتح الإعدادات → الموقع → «السماح دائماً».",
                "فتح الإعدادات",
                "خروج");

            if (!openSettings)
                return false;

            OpenLocationSettings();
            await Task.Delay(1000);
        }
    }

    private void OpenLocationSettings()
    {
#if ANDROID
        var intent = new Intent(Settings.ActionApplicationDetailsSettings);
        intent.SetData(Android.Net.Uri.FromParts("package", Android.App.Application.Context.PackageName, null));
        intent.AddFlags(ActivityFlags.NewTask);
        Android.App.Application.Context.StartActivity(intent);
#else
        var url = new NSUrl(UIApplication.OpenSettingsUrlString);
        if (UIApplication.SharedApplication.CanOpenUrl(url))
            UIApplication.SharedApplication.OpenUrl(url);
#endif
    }

    // =============================================
    //  Attendance buttons
    // =============================================
    private void OnAttendanceClicked(object sender, EventArgs e)
        => _ = OnAttendanceClickedUnified("IN");

    private void OnAttendanceExitClicked(object sender, EventArgs e)
        => _ = OnAttendanceClickedUnified("OUT");

    // =============================================
    //  Core attendance logic (محسّن)
    // =============================================
    private async Task OnAttendanceClickedUnified(string action)
    {
        try
        {
            lOADER.IsVisible = true;

            // ✅ 0. التحقق من وجود فرع معين
            if (!Preferences.ContainsKey("BranchId"))
            {
                await _alertService.ShowAlertAsync(
                    "⚠️ تنبيه",
                    "لم يتم تحديد فرع لك\nبرجاء التواصل مع قسم الموارد البشرية",
                    "حسناً");
                return;
            }

            int branchId = Preferences.Get("BranchId", 0);
            string branchName = Preferences.Get("BranchName", "");
            double companyLat = Preferences.Get("CompanyLatitude", 0.0);
            double companyLng = Preferences.Get("CompanyLongitude", 0.0);
            double allowedRadius = Preferences.Get("CompanyRadius", 0.0);
            bool requireWifi = Preferences.Get("RequireWifi", false);
            bool requireGps = Preferences.Get("RequireGps", true);
            string companyWifiSSID = Preferences.Get("CompanyWifiSSID", "");

            // ✅ 1. صلاحية الموقع
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await _alertService.ShowAlertAsync(
                        "الموقع غير مسموح",
                        "يجب السماح للتطبيق بالوصول إلى الموقع",
                        "فتح الإعدادات");
                    OpenLocationSettings();
                    return;
                }
            }

            // ✅ 2. جلب الموقع (لو GPS مطلوب)
            double employeeLat = 0;
            double employeeLng = 0;
            bool isMockLocation = false;

            if (requireGps)
            {
                Location? location;
                try
                {
                    location =
                        await Geolocation.GetLastKnownLocationAsync()
                        ?? await Geolocation.GetLocationAsync(
                            new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

                    if (location == null)
                    {
                        await _alertService.ShowAlertAsync(
                            "تعذر التحديد",
                            "لم نتمكن من تحديد موقعك الحالي",
                            "تمام");
                        return;
                    }

                    employeeLat = location.Latitude;
                    employeeLng = location.Longitude;
                    isMockLocation = location.IsFromMockProvider;
                }
                catch (FeatureNotEnabledException)
                {
                    await _alertService.ShowAlertAsync(
                        "الموقع مغلق",
                        "يجب تشغيل خدمات الموقع من إعدادات الجهاز",
                        "فتح الإعدادات");
                    OpenLocationSettings();
                    return;
                }

                // ✅ حساب المسافة
                double distanceMeters = Haversine(employeeLat, employeeLng, companyLat, companyLng);

                // ✅ التحقق من المسافة قبل إرسال الطلب
                if (distanceMeters > allowedRadius)
                {
                    await _alertService.ShowAlertAsync(
                        "تنبيه ⚠️",
                        $"أنت خارج نطاق فرع {branchName}\nالمسافة الحالية: {distanceMeters:F1} متر\nالحد المسموح: {allowedRadius} متر",
                        "تمام");
                    return;
                }
            }

            // ✅ 3. Wi-Fi (لو مطلوب)
            string ssid = string.Empty;
            string bssid = string.Empty;

#if ANDROID
            if (requireWifi)
            {
                var wifiManager = (Android.Net.Wifi.WifiManager?)Android.App.Application.Context
                    .GetSystemService(Android.Content.Context.WifiService);

                if (wifiManager?.ConnectionInfo != null)
                {
                    ssid = wifiManager.ConnectionInfo.SSID?.Trim('"').Replace("\\u0022", "") ?? "";
                    bssid = wifiManager.ConnectionInfo.BSSID ?? "";
                }

                if (string.IsNullOrEmpty(ssid))
                {
                    await _alertService.ShowAlertAsync(
                        "تنبيه ⚠️",
                        $"يجب الاتصال بشبكة Wi-Fi: {companyWifiSSID}",
                        "تمام");
                    return;
                }

                // ✅ التحقق من اسم الواي فاي
                if (!string.Equals(ssid, companyWifiSSID, StringComparison.OrdinalIgnoreCase))
                {
                    await _alertService.ShowAlertAsync(
                        "واي فاي غير صحيح ⚠️",
                        $"أنت متصل بـ: {ssid}\nالمطلوب: {companyWifiSSID}",
                        "تمام");
                    return;
                }
            }
#endif

#if IOS
if (requireWifi)
{
    // ⚠️ iOS لا يدعم WiFi detection بشكل موثوق في الخلفية
    // نعلم المستخدم ونستمر بدون WiFi check
    await _alertService.ShowAlertAsync(
        "ملاحظة ℹ️",
        "iOS لا يدعم التحقق من WiFi في الخلفية.\nسيتم استخدام GPS فقط للتحقق من الموقع.",
        "فهمت");
    
    // تعطيل WiFi requirement
    requireWifi = false;
    ssid = "iOS-Disabled";
    bssid = "iOS-Disabled";
}
#endif


            // ✅ 4. إرسال للسيرفر
            var payload = new
            {
                Latitude = employeeLat,
                Longitude = employeeLng,
                SSID = ssid,
                BSSID = bssid,
                IsMockLocation = isMockLocation,
                Action = action,
                DeviceTime = DateTimeOffset.UtcNow,
                IsOfflineSync = false
            };

            var response = await LoginPage.PostWithTokenAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/AttendancePolicy/verify",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            var resultString = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(resultString).RootElement;

            bool success = root.GetProperty("success").GetBoolean();
            string decision = root.GetProperty("decision").GetString() ?? "";

            if (success)
            {
                // ✅ جلب المسافة من الـ response
                double finalDistance = root.TryGetProperty("distance", out var distProp)
                    ? distProp.GetDouble()
                    : 0;

                string actionText = action == "IN" ? "الحضور" : "الانصراف";
                string distanceText = requireGps ? $"\nالمسافة: {finalDistance:F1} متر" : "";

                await _alertService.ShowAlertAsync(
                    "نجاح ✅",
                    $"تم تسجيل {actionText} بنجاح{distanceText}\nالفرع: {branchName}\nالحالة: {decision}",
                    "تمام");

                // ✅ تشغيل/إيقاف Location Service
                HandleLocationService(action, companyLat, companyLng, allowedRadius);
            }
            else
            {
                string reason = root.TryGetProperty("reason", out var reasonProp)
                    ? reasonProp.GetString() ?? decision
                    : decision;

                await _alertService.ShowAlertAsync(
                    "تنبيه ⚠️",
                    $"تعذر تسجيل {(action == "IN" ? "الحضور" : "الانصراف")}\nالسبب: {reason}",
                    "تمام");
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlertAsync("خطأ غير متوقع", ex.Message, "تمام");
        }
        finally
        {
            lOADER.IsVisible = false;
        }
    }

    // =============================================
    //  Quick Actions Navigation
    // =============================================

    // إجازة
    private async void OnLeaveClicked(object sender, TappedEventArgs e)
        => await NavigateTo(new NyLeaveRequests());

    // كشف الحضور
    private async void OnMyAttendanceClicked(object sender, TappedEventArgs e)
        => await NavigateTo(new MyAttendance());

    // سلفة
    private async void OnAdvanceClicked(object sender, TappedEventArgs e)
        => await NavigateTo(new MyAdvances());

    // تصريح
    private async void OnPermissionClicked(object sender, TappedEventArgs e)
        => await NavigateTo(new MyPermissions());

    // الراتب
    private async void OnSalaryClicked(object sender, TappedEventArgs e)
        => await NavigateTo(new MySallary());

    // تقييمات
    private async void OnEvaluationClicked(object sender, TappedEventArgs e)
        => await NavigateTo(new MyEvaluations());

    // رسائل
    private async void OnMessagesClicked(object sender, TappedEventArgs e)
        => await NavigateTo(new MyMessages());

    // مأمورية
    private async void OnMissionClicked(object sender, TappedEventArgs e)
        => await NavigateTo(new MyMissionRequest());

    // =============================================
    //  Navigation helper (يدعم FlyoutPage)
    // =============================================
    private static async Task NavigateTo(Page page)
    {
        if (App.Current?.MainPage is FlyoutPage { Detail: NavigationPage nav })
            await nav.PushAsync(page);
        else if (App.Current?.MainPage is NavigationPage rootNav)
            await rootNav.PushAsync(page);
        else
            await Microsoft.Maui.Controls.Application.Current!.MainPage!.Navigation.PushAsync(page);
    }

    // =============================================
    //  Location foreground service helpers
    // =============================================
    private static void HandleLocationService(string action, double companyLat, double companyLng, double radiusMeters)
    {
#if ANDROID
        try
        {
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(LocationForegroundService));

            if (action == "IN")
            {
                // ✅ تشغيل Location Tracking + Background Sync معاً
                intent.SetAction("ACTION_START_LOCATION_SERVICE");
                intent.PutExtra("CompanyLatitude", companyLat);
                intent.PutExtra("CompanyLongitude", companyLng);
                intent.PutExtra("CompanyRadius", radiusMeters);
                context.StartForegroundService(intent);
                Console.WriteLine("✅ Android Location Service Started");
            }
            else
            {
                // ✅ إيقاف كل شيء
                intent.SetAction("ACTION_STOP_LOCATION_SERVICE");
                context.StartService(intent);
                Console.WriteLine("⏹️ Android Location Service Stopped");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Android Location Service Error: {ex.Message}");
        }
#endif

#if IOS
    try
    {
        var prefs = new CompanyPrefs
        {
            CompanyLat = companyLat,
            CompanyLng = companyLng,
            RadiusMeters = radiusMeters
        };
 
        if (action == "IN")
        {
            // ✅ استخدام Singleton Service بدلاً من instance variable
            HSEM.Platforms.iOS.iOSPlatformServices.StartLocationTracking(prefs);
            HSEM.Platforms.iOS.Services.iOSBackgroundSyncService.Instance.EnableBackgroundSync();
            Console.WriteLine("✅ iOS Location Service Started");
        }
        else
        {
            // ✅ إيقاف التتبع
            HSEM.Platforms.iOS.iOSPlatformServices.StopLocationTracking();
            Console.WriteLine("⏹️ iOS Location Service Stopped");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ iOS Location Service Error: {ex.Message}");
    }
#endif
    }


    // =============================================
    //  Haversine formula
    // =============================================
    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371e3;
        double φ1 = lat1 * Math.PI / 180;
        double φ2 = lat2 * Math.PI / 180;
        double Δφ = (lat2 - lat1) * Math.PI / 180;
        double Δλ = (lon2 - lon1) * Math.PI / 180;
        double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                   Math.Cos(φ1) * Math.Cos(φ2) *
                   Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    // =============================================
    //  Toolbar & Back button
    // =============================================
    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        if (App.Current?.MainPage is FlyoutPage { Detail: NavigationPage nav })
            await nav.PopAsync();
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        NavigationHelper.ToggleFlyoutMenu();
    }

    protected override bool OnBackButtonPressed()
    {
        if (App.Current?.MainPage is FlyoutPage { Detail: NavigationPage nav })
            MainThread.InvokeOnMainThreadAsync(async () => await nav.PopAsync());
        return true;
    }
}