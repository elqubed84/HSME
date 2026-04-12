using HSEM.Interfaces;
using HSEM.Services;
using Plugin.Firebase.CloudMessaging;
using Plugin.Maui.Biometric;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HSEM.Views;

public partial class LoginPage : ContentPage
{
    private readonly IPopupService _alertService;

    public LoginPage()
    {
        InitializeComponent();
        _alertService = new PopupService();

        // ✅ تحقق من Token أولاً قبل Auto Login
        this.Dispatcher.Dispatch(async () =>
        {
            await CheckExistingSessionAsync();
        });
    }

    // =============================================
    //  ✅ جديد: التحقق من الجلسة الموجودة
    // =============================================
    private async Task CheckExistingSessionAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("AccessToken");
            var userId = Preferences.Get("UserId", null);
            var expiration = Preferences.Get("AccessTokenExpiration", DateTime.MinValue);

            // ✅ لو في Token صالح → ادخل مباشرة
            if (!string.IsNullOrEmpty(token)
                && !string.IsNullOrEmpty(userId)
                && DateTime.UtcNow < expiration)
            {
                // Token صالح → ادخل مباشرة
                lOADER.IsVisible = true;

#if ANDROID
                StartLocationServiceInBackground();
#endif
                await Navigation.PushAsync(new Dashboard());
                lOADER.IsVisible = false;
                return;
            }

            // ✅ لو Token منتهي → حاول تجدده
            if (!string.IsNullOrEmpty(token)
                && !string.IsNullOrEmpty(userId)
                && DateTime.UtcNow >= expiration)
            {
                bool refreshed = await TryRefreshTokenAsync();
                if (refreshed)
                {
                    lOADER.IsVisible = true;
#if ANDROID
                    StartLocationServiceInBackground();
#endif
                    await Navigation.PushAsync(new Dashboard());
                    lOADER.IsVisible = false;
                    return;
                }
                // لو فشل التجديد → امسح البيانات القديمة
                await ClearStoredCredentialsAsync();
            }

            // ✅ لو مفيش Token → حاول Auto Login
            var usern = await SecureStorage.Default.GetAsync("UserEmail");
            var pass = await SecureStorage.Default.GetAsync("UserPassword");

            if (!string.IsNullOrEmpty(usern) && !string.IsNullOrEmpty(pass))
            {
                emailEntry.Text = usern;
                passwordEntry.Text = pass;
                await AutoLogin(usern, pass);
            }
            else
            {
                _ = AttemptBiometricLoginAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CheckExistingSession error: {ex.Message}");
        }
    }

    // =============================================
    //  ✅ جديد: محاولة تجديد Token
    // =============================================
    private async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            var refreshToken = await SecureStorage.Default.GetAsync("RefreshToken");
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var httpClient = HttpClientFactory.Instance;
            var response = await httpClient.PostAsJsonAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/auth/refresh",
                refreshToken);

            if (!response.IsSuccessStatusCode)
                return false;

            var resultString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultString);
            var result = doc.RootElement;

            string newToken = result.GetProperty("accessToken").GetString() ?? "";
            string newRefreshToken = result.GetProperty("refreshToken").GetString() ?? "";
            DateTime newExpiry = result.GetProperty("accessTokenExpiration").GetDateTime();

            if (string.IsNullOrEmpty(newToken))
                return false;

            await SecureStorage.Default.SetAsync("AccessToken", newToken);
            await SecureStorage.Default.SetAsync("RefreshToken", newRefreshToken);
            Preferences.Set("AccessTokenExpiration", newExpiry);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Refresh token failed: {ex.Message}");
            return false;
        }
    }

    // =============================================
    //  ✅ جديد: مسح بيانات الاعتماد القديمة
    // =============================================
    private async Task ClearStoredCredentialsAsync()
    {
        SecureStorage.Default.Remove("AccessToken");
        SecureStorage.Default.Remove("RefreshToken");
        Preferences.Remove("AccessTokenExpiration");
        Preferences.Remove("UserId");

        // ✅ لا تمسح Email و Password عشان Auto Login
        // SecureStorage.Default.Remove("UserEmail");
        // SecureStorage.Default.Remove("UserPassword");
    }

    private async Task AttemptBiometricLoginAsync()
    {
        try
        {
            var savedEmail = await SecureStorage.Default.GetAsync("UserEmail");
            var savedPassword = await SecureStorage.Default.GetAsync("UserPassword");

            if (string.IsNullOrEmpty(savedEmail) || string.IsNullOrEmpty(savedPassword))
                return;

            var result = await BiometricAuthenticationService.Default.AuthenticateAsync(
                new AuthenticationRequest { Title = "تأكيد هويتك", NegativeText = "إلغاء" },
                CancellationToken.None);

            if (result.Status == BiometricResponseStatus.Success)
            {
                emailEntry.Text = savedEmail;
                passwordEntry.Text = savedPassword;
                SAAVE.IsChecked = true;
                await AutoLogin(savedEmail, savedPassword);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Biometric login failed: {ex.Message}");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
        => await PerformLogin(emailEntry.Text?.Trim(), passwordEntry.Text?.Trim());

    private async Task AutoLogin(string email, string password)
        => await PerformLogin(email, password, isAutoLogin: true);

    private async Task PerformLogin(string email, string password, bool isAutoLogin = false)
    {
        try
        {
            lOADER.IsVisible = true;

            string uniqueDeviceId = GenerateDeviceFingerprint();

            // ✅ طلب permission بدون إيقاف
            var locStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locStatus != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            // ✅ تحقق من النت أولاً
            bool hasInternet = await HasRealInternetAsync();

            if (!hasInternet)
            {
                // ===== مفيش نت =====
                var savedToken = await SecureStorage.Default.GetAsync("AccessToken");
                var savedUserId = Preferences.Get("UserId", null);
                var expiration = Preferences.Get("AccessTokenExpiration", DateTime.MinValue);

                // ✅ تحقق من صلاحية Token
                if (!string.IsNullOrEmpty(savedToken)
                    && !string.IsNullOrEmpty(savedUserId)
                    && DateTime.UtcNow < expiration)
                {
                    // ✅ في بيانات محفوظة وصالحة → ادخل بدون سيرفر
                    if (!isAutoLogin)
                        await _alertService.ShowAlertAsync(
                            "وضع بدون إنترنت 📴",
                            "تم تسجيل الدخول من البيانات المحفوظة\nسيتم المزامنة عند عودة الإنترنت",
                            "تمام");
#if ANDROID
                    StartLocationServiceInBackground();
#endif
                    await Navigation.PushAsync(new Dashboard());
                    return;
                }
                else
                {
                    if (!isAutoLogin)
                        await _alertService.ShowAlertAsync(
                            "لا يوجد إنترنت 📴",
                            "يجب الاتصال بالإنترنت عند تسجيل الدخول لأول مرة أو بعد انتهاء الجلسة",
                            "تمام");
                    return;
                }
            }

            // ===== في نت → تسجيل دخول عادي =====
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                if (!isAutoLogin)
                    await _alertService.ShowAlertAsync("تنبيه", "يرجى إدخال البريد وكلمة المرور", "موافق");
                return;
            }

            var httpClient = HttpClientFactory.Instance;
            var payload = new
            {
                LoginInput = email,
                Password = password,
                RememberMe = !isAutoLogin,
                DeviceId = uniqueDeviceId
            };

            var response = await httpClient.PostAsJsonAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/auth/login",
                payload);
            var resultString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(resultString);
            var result = doc.RootElement;

            if (response.IsSuccessStatusCode)
            {
                string welcome = result.GetProperty("message").GetString() ?? "";
                string userId = result.GetProperty("userId").GetString() ?? "";
                string accessToken = result.GetProperty("accessToken").GetString() ?? "";
                string refreshToken = result.GetProperty("refreshToken").GetString() ?? "";
                DateTime accessTokenExpiry = result.GetProperty("accessTokenExpiration").GetDateTime();

                // ===== ✅ معالجة الفروع الجديدة =====
                bool hasBranch = false;
                bool requireBranchAssignment = false;
                string branchWarning = null;

                if (result.TryGetProperty("hasBranch", out JsonElement hasBranchElement))
                {
                    hasBranch = hasBranchElement.GetBoolean();
                }

                if (result.TryGetProperty("requireBranchAssignment", out JsonElement requireElement))
                {
                    requireBranchAssignment = requireElement.GetBoolean();
                }

                if (result.TryGetProperty("branchWarning", out JsonElement warningElement))
                {
                    branchWarning = warningElement.GetString();
                }

                // ===== ✅ حفظ بيانات الفرع المعين =====
                if (hasBranch && result.TryGetProperty("branch", out JsonElement branchElement))
                {
                    Preferences.Set("BranchId", branchElement.GetProperty("id").GetInt32());
                    Preferences.Set("BranchName", branchElement.GetProperty("name").GetString() ?? "");
                    Preferences.Set("CompanyLatitude", branchElement.GetProperty("latitude").GetDouble());
                    Preferences.Set("CompanyLongitude", branchElement.GetProperty("longitude").GetDouble());
                    Preferences.Set("CompanyRadius", branchElement.GetProperty("radiusMeters").GetDouble());

                    if (branchElement.TryGetProperty("requireWifi", out JsonElement wifiElement))
                        Preferences.Set("RequireWifi", wifiElement.GetBoolean());

                    if (branchElement.TryGetProperty("requireGps", out JsonElement gpsElement))
                        Preferences.Set("RequireGps", gpsElement.GetBoolean());

                    if (branchElement.TryGetProperty("wifiSSID", out JsonElement ssidElement))
                        Preferences.Set("CompanyWifiSSID", ssidElement.GetString() ?? "");
                }
                else
                {
                    // ✅ مسح البيانات القديمة لو مفيش فرع
                    Preferences.Remove("BranchId");
                    Preferences.Remove("BranchName");
                    Preferences.Remove("CompanyLatitude");
                    Preferences.Remove("CompanyLongitude");
                    Preferences.Remove("CompanyRadius");
                    Preferences.Remove("RequireWifi");
                    Preferences.Remove("RequireGps");
                    Preferences.Remove("CompanyWifiSSID");
                }

                // ===== ✅ معالجة الـ Roles =====
                var roles = new List<string>();
                bool isAdmin = false;

                if (result.TryGetProperty("roles", out JsonElement rolesElement)
                    && rolesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        var r = role.GetString();
                        if (r != null) roles.Add(r);
                    }
                    isAdmin = roles.Contains("Admin") || roles.Contains("HR");
                }

                if (isAdmin)
                    Preferences.Set("UserRole", true);

                // ===== حفظ البيانات الأساسية =====
                if (!string.IsNullOrEmpty(userId))
                    Preferences.Set("UserId", userId);

                if (!string.IsNullOrEmpty(accessToken))
                    await SecureStorage.Default.SetAsync("AccessToken", accessToken);

                if (!string.IsNullOrEmpty(refreshToken))
                    await SecureStorage.Default.SetAsync("RefreshToken", refreshToken);

                Preferences.Set("AccessTokenExpiration", accessTokenExpiry);

                if (SAAVE.IsChecked ?? false)
                {
                    await SecureStorage.Default.SetAsync("UserPassword", password);
                    await SecureStorage.Default.SetAsync("UserEmail", email);
                }

                // ✅ FCM في background
                _ = GetValidFcmTokenAsync();

                // ===== 🔴 معالجة حالة "موظف بدون فرع" =====
                if (requireBranchAssignment && !isAdmin)
                {
                    // ✅ لا تشغل الـ Service
                    // ✅ عرض رسالة تحذيرية

                    if (!isAutoLogin)
                    {
                        await _alertService.ShowAlertAsync(
                            "⚠️ تنبيه مهم",
                            branchWarning ?? "لم يتم تحديد فرع لك\nبرجاء التواصل مع الإدارة لتفعيل الحضور",
                            "حسناً");
                    }

                    // ✅ الانتقال للـ Dashboard لكن بدون تفعيل الحضور
                    await Navigation.PushAsync(new Dashboard());
                    return;
                }

                // ===== ✅ الحالة العادية (عنده فرع أو Admin) =====
#if ANDROID
                StartLocationServiceInBackground();
#endif

                // ===== ✅ الحالة العادية (عنده فرع أو Admin) =====
                if (!isAutoLogin)
                {
                    string finalMessage = welcome;
                    if (hasBranch && !string.IsNullOrEmpty(Preferences.Get("BranchName", "")))
                    {
                        finalMessage += $"\n📍 فرعك: {Preferences.Get("BranchName", "")}";
                    }

                    await _alertService.ShowAlertAsync("نجاح 🎉", finalMessage, "تمام");
                }

                // ✅ Navigation محسّن
                try
                {
                    // ✅ التحقق من MainPage
                    if (App.Current?.MainPage == null)
                    {
                        Console.WriteLine("⚠️ MainPage is null, creating new FlyoutPage");
                        App.Current.MainPage = new AppFlyoutPage();
                    }

                    // ✅ محاولة Cast لـ FlyoutPage
                    var flyoutPage = App.Current.MainPage as FlyoutPage;

                    if (flyoutPage != null)
                    {
                        // ✅ في FlyoutPage موجود
                        Console.WriteLine("✅ Navigating via FlyoutPage.Detail");
                        flyoutPage.Detail = new NavigationPage(new Dashboard());
                        flyoutPage.IsPresented = false;
                    }
                    else
                    {
                        // ⚠️ MainPage مش FlyoutPage → أعد إنشاءها
                        Console.WriteLine("⚠️ MainPage is not FlyoutPage, recreating...");

                        App.Current.MainPage = new FlyoutPage
                        {
                            Flyout = new AppFlyoutPageFlyout(),
                            Detail = new NavigationPage(new Dashboard())
                        };
                    }
                }
                catch (Exception navEx)
                {
                    Console.WriteLine($"❌ Navigation error: {navEx.Message}");

                    // ✅ Last resort: أعد إنشاء كل شيء
                    try
                    {
                        App.Current.MainPage = new FlyoutPage
                        {
                            Flyout = new AppFlyoutPageFlyout(),
                            Detail = new NavigationPage(new Dashboard())
                        };

                        Console.WriteLine("✅ MainPage reset successful");
                    }
                    catch (Exception resetEx)
                    {
                        Console.WriteLine($"❌ CRITICAL: {resetEx.Message}");

                        await _alertService.ShowAlertAsync(
                            "خطأ في الانتقال",
                            "برجاء إغلاق التطبيق وإعادة فتحه.",
                            "حسناً");
                    }
                }
            }
            else
            {
                string errorMessage = result.TryGetProperty("message", out var msg)
                    ? msg.GetString() ?? "خطأ غير معروف"
                    : "فشل تسجيل الدخول";

                if (!isAutoLogin)
                    await _alertService.ShowAlertAsync("خطأ 😅", errorMessage, "تمام");
            }
        }
        catch (Exception ex)
        {
            if (!isAutoLogin)
                await _alertService.ShowAlertAsync("خطأ", ex.Message, "موافق");
        }
        finally
        {
            lOADER.IsVisible = false;
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
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, "https://elnagarygroup-001-site1.ktempurl.com"),
                HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

#if ANDROID
    private static void StartLocationServiceInBackground()
    {
        try
        {
            var context = global::Android.App.Application.Context;
            var intent = new global::Android.Content.Intent(
                context,
                typeof(HSEM.Platforms.Android.Services.LocationForegroundService));
            intent.SetAction("ACTION_START_SYNC_ONLY");

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else
                context.StartService(intent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Service start error: {ex.Message}");
        }
    }
#endif

    public static string GenerateDeviceFingerprint()
    {
        var rawData =
            $"{DeviceInfo.Current.Platform}|" +
            $"{DeviceInfo.Current.Model}|" +
            $"{DeviceInfo.Current.Manufacturer}|" +
            $"{DeviceInfo.Current.Version.Major}|" +
            $"{DeviceInfo.Current.Idiom}";
        return ComputeSha256Hash(rawData);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (byte b in bytes)
            builder.Append(b.ToString("x2"));
        return builder.ToString();
    }

    public static async Task<bool> EnsureLocationPermissionsAsync()
    {
        var fineStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (fineStatus != PermissionStatus.Granted)
            fineStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return fineStatus == PermissionStatus.Granted;
    }

    private async Task SendTokenToServerAsync(string fcmToken)
    {
        if (string.IsNullOrWhiteSpace(fcmToken)) return;
        try
        {
            var client = HttpClientFactory.Instance;
            var accessToken = await SecureStorage.Default.GetAsync("AccessToken");
            if (!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

            await client.PostAsJsonAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/AddToken/AddTokenUser",
                new { Token = fcmToken });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send FCM token: {ex.Message}");
        }
    }

    private async Task<string?> GetValidFcmTokenAsync()
    {
        try
        {
            string? existingToken = Preferences.Get("GoogleFcmToken", null);
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            string? token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                if (existingToken != token)
                {
                    Preferences.Set("GoogleFcmToken", token);
                    await SendTokenToServerAsync(token);
                }
                return token;
            }

            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                Preferences.Set("GoogleFcmToken", token);
                await SendTokenToServerAsync(token);
                return token;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error getting valid FCM token: {ex.Message}");
        }
        return null;
    }

    public static async Task EnsureAccessTokenValidAsync()
    {
        var expiration = Preferences.Get("AccessTokenExpiration", DateTime.MinValue);
        if (DateTime.UtcNow >= expiration)
        {
            var refreshToken = await SecureStorage.Default.GetAsync("RefreshToken") ?? "";
            if (string.IsNullOrEmpty(refreshToken))
                throw new Exception("Refresh token not available");

            var httpClient = HttpClientFactory.Instance;
            var response = await httpClient.PostAsJsonAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/auth/refresh", refreshToken);

            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(resultString);
                var result = doc.RootElement;

                string newToken = result.GetProperty("accessToken").GetString() ?? "";
                DateTime newExpiry = result.GetProperty("accessTokenExpiration").GetDateTime();

                await SecureStorage.Default.SetAsync("AccessToken", newToken);
                Preferences.Set("AccessTokenExpiration", newExpiry);
            }
            else
            {
                throw new Exception("Unable to refresh token");
            }
        }
    }

    public static async Task<HttpResponseMessage> PostWithTokenAsync(string url, HttpContent content)
    {
        await EnsureAccessTokenValidAsync();
        var httpClient = HttpClientFactory.Instance;
        string token = await SecureStorage.Default.GetAsync("AccessToken") ?? "";
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return await httpClient.PostAsync(url, content);
    }

    public static async Task<HttpResponseMessage> GetWithTokenAsync(string url)
    {
        await EnsureAccessTokenValidAsync();
        var httpClient = HttpClientFactory.Instance;
        string token = await SecureStorage.Default.GetAsync("AccessToken") ?? "";
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return await httpClient.GetAsync(url);
    }
}