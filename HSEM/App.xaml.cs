using HSEM.Views;
using HSEM.Views.Forms;
using Controls.UserDialogs.Maui;
using HSEM.Interfaces;
using HSEM.Services;
using Java.Security;
using Microsoft.Maui.Platform;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Permissions = Microsoft.Maui.ApplicationModel.Permissions;
#if ANDROID
namespace HSEM
{
    public partial class App : Application
    {
        private readonly IPopupService _alertService;
		public static string ImageServerPath { get; } = "https://cdn.syncfusion.com/essential-ui-kit-for-.net-maui/common/uikitimages/";

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1cdnRdR2leVEFyXkVWYEs=");
            InitializeComponent();

            _alertService = new PopupService();

            // الاشتراك في حدث تغيير حالة الاتصال
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await CheckInternetConnectionAsync();
            });
            
            MainPage = new AppFlyoutPage();
            
        }
        private bool _wasOffline = false;
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private async Task CheckInternetConnectionAsync()
        {
            var networkAccess = Connectivity.Current.NetworkAccess;

            if (networkAccess != NetworkAccess.Internet)
            {
                HandleNoInternet();
                return;
            }

            // فحص إنترنت حقيقي
            var hasRealInternet = await HasRealInternetAsync();

            if (hasRealInternet)
            {
                HandleInternetRestored();
            }
            else
            {
                HandleNoInternet();
            }
        }
        private async Task<bool> HasRealInternetAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Head,
                    "https://elnagarygroup-001-site1.ktempurl.com/api/health"
                );

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        private void HandleNoInternet()
        {
            SetStatusBarColor(Colors.Red);

            if (!_wasOffline)
            {
                _wasOffline = true;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UserDialogs.Instance.Alert(
                        "لا يوجد اتصال حقيقي بالإنترنت.\nتحقق من الشبكة أو البيانات.",
                        "تنبيه",
                        "حسناً"
                    );
                });
            }
        }
        private void HandleInternetRestored()
        {
            SetStatusBarColor(Colors.Green);

            if (_wasOffline)
            {
                _wasOffline = false;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UserDialogs.Instance.ShowToast("✅ تم الاتصال بالسيرفر بنجاح");
                });
            }
        }
        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            await CheckInternetConnectionAsync();
        }
        private void SetStatusBarColor(Color color)
        {
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity != null)
            {
                var window = activity.Window;
                window.SetStatusBarColor(color.ToPlatform());
            }
#endif
        }


        protected override void OnStart()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await CheckInternetConnectionAsync();
                await AskForRelevantPermissionsAsync();
            });

        }
        private async Task AskForRelevantPermissionsAsync()
        {
            await AskForPermissionAsync<Permissions.LocationWhenInUse>();
            //await AskForPermissionAsync<Permissions.LocationAlways>();
            await AskForPermissionAsync<Permissions.Camera>();
            await AskForPermissionAsync<Permissions.Media>();
            await AskForPermissionAsync<Permissions.NetworkState>();
            //await AskForPermissionAsync<Permissions.AccessWifiState>();
            await RequestNotification();
        }
        private async Task AskForPermissionAsync<TPermission>()
            where TPermission : BasePermission, new()
        {
            var result = await CheckStatusAsync<TPermission>();
            if (result != PermissionStatus.Granted)
            {
                //await App.AlertSvc.ShowAlertAsync("Attention", "You must allow this Permission for the application to work properly"+result.ToString());
                await RequestAsync<TPermission>();

            }
        }
        async Task RequestNotification()
        {
            if (DeviceInfo.Platform != DevicePlatform.Android)
                return;

            var status = PermissionStatus.Unknown;

            if (DeviceInfo.Version.Major >= 12)
            {
                status = await Permissions.CheckStatusAsync<MyNotificationPermission>();

                if (status == PermissionStatus.Granted)
                    return;

                if (Permissions.ShouldShowRationale<MyNotificationPermission>())
                {
                    await _alertService.ShowAlertAsync(
                        "مطلوب إذن",
                        "يجب السماح بإذن الإشعارات لكي يعمل التطبيق بشكل صحيح.",
                        "حسنًا");
                }

                status = await Permissions.RequestAsync<MyNotificationPermission>();


            }
            else
            {
                status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                    return;

                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                {
                    await _alertService.ShowAlertAsync(
                        "مطلوب إذن",
                        "يجب السماح بهذا الإذن لكي يعمل التطبيق بدون مشاكل.",
                        "حسنًا");
                }

                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();


            }


            if (status != PermissionStatus.Granted)
                await _alertService.ShowAlertAsync(
                    "مطلوب إذن",
                    "إذن الموقع مطلوب لتمكين البحث عبر البلوتوث. نحن لا نقوم بتخزين أو استخدام موقعك بأي شكل.",
                    "حسنًا");
        }
    }
}
#endif