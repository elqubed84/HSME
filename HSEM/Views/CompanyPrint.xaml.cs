using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using HSEM.Helper;
using HSEM.Interfaces;
using HSEM.Models;
using HSEM.Services;
using Syncfusion.Maui.Buttons;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace HSEM.Views;

public partial class CompanyPrint : ContentPage
{
    private readonly IPopupService _alertService;
    private readonly IApiService _apiService;

    public ObservableCollection<BranchItem> Branches { get; set; }
    private BranchItem? _selectedBranch;

    public CompanyPrint()
    {
        _alertService = new PopupService();
        _apiService = new ApiService();
        Branches = new ObservableCollection<BranchItem>();
        InitializeComponent();

        BranchesList.ItemsSource = Branches;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBranches();
    }

    // =============================================
    // تحميل قائمة الفروع
    // =============================================
    private async Task LoadBranches()
    {
        try
        {
            Loader.IsVisible = true;
            Loader.IsRunning = true;

            var token = await SecureStorage.Default.GetAsync("AccessToken");
            var client = HttpClientFactory.Instance;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/AttendancePolicy");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var branches = JsonSerializer.Deserialize<List<BranchItem>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Branches.Clear();
                if (branches != null)
                {
                    foreach (var branch in branches)
                    {
                        Branches.Add(branch);
                    }
                }

                NoBranchesLabel.IsVisible = Branches.Count == 0;
            }
            else
            {
                await _alertService.ShowAlertAsync("خطأ", "فشل تحميل الفروع", "حسناً");
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlertAsync("خطأ", ex.Message, "حسناً");
        }
        finally
        {
            Loader.IsRunning = false;
            Loader.IsVisible = false;
        }
    }

    // =============================================
    // إضافة فرع جديد
    // =============================================
    private async void OnAddBranchClicked(object sender, EventArgs e)
    {
        _selectedBranch = null;
        await ShowBranchDialog(isEdit: false);
    }

    // =============================================
    // تعديل فرع
    // =============================================
    private async void OnEditBranchClicked(object sender, EventArgs e)
    {
        var button = sender as SfButton;
        var branch = button?.BindingContext as BranchItem;

        if (branch == null) return;

        _selectedBranch = branch;
        await ShowBranchDialog(isEdit: true);
    }

    // =============================================
    // حذف فرع
    // =============================================
    private async void OnDeleteBranchClicked(object sender, EventArgs e)
    {
        var button = sender as SfButton; // Syncfusion button
        var branch = button?.BindingContext as BranchItem;

        if (branch == null)
        {
            await _alertService.ShowAlertAsync("خطأ", "لم يتم العثور على الفرع.", "حسناً");
            return;
        }

        bool confirm = await DisplayAlert(
            "تأكيد الحذف",
            $"هل تريد حذف فرع '{branch.Name}'؟",
            "حذف",
            "إلغاء");

        if (!confirm) return;

        try
        {
            Loader.IsVisible = true;
            Loader.IsRunning = true;

            var token = await SecureStorage.Default.GetAsync("AccessToken");
            var client = HttpClientFactory.Instance;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync(
                $"https://elnagarygroup-001-site1.ktempurl.com/api/AttendancePolicy/{branch.Id}");

            if (response.IsSuccessStatusCode)
            {
                await _alertService.ShowAlertAsync("نجاح", "تم حذف الفرع بنجاح", "حسناً");
                await LoadBranches();
            }
            else
            {
                await _alertService.ShowAlertAsync("خطأ", "فشل حذف الفرع", "حسناً");
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlertAsync("خطأ", ex.Message, "حسناً");
        }
        finally
        {
            Loader.IsRunning = false;
            Loader.IsVisible = false;
        }
    }

    // =============================================
    // عرض نافذة إضافة/تعديل الفرع
    // =============================================
    private async Task ShowBranchDialog(bool isEdit)
    {
        try
        {
            // 1️⃣ تحقق من الاتصال بالواي فاي
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet ||
                !Connectivity.Current.ConnectionProfiles.Contains(ConnectionProfile.WiFi))
            {
                bool openSettings = await _alertService.ShowAlertAsyncbool(
                    "تنبيه",
                    "يجب الاتصال بشبكة WiFi أولاً",
                    "فتح الإعدادات");

                if (openSettings)
                    AppInfo.ShowSettingsUI();

                return;
            }

            // 2️⃣ تحقق من إذن الموقع
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    bool openSettings = await _alertService.ShowAlertAsyncbool(
                        "تنبيه",
                        "يجب السماح بالوصول إلى الموقع الدقيق",
                        "فتح الإعدادات");

                    if (openSettings)
                        AppInfo.ShowSettingsUI();

                    return;
                }
            }

            // 3️⃣ تحقق من تفعيل GPS
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.High));
            }

            if (location == null)
            {
                bool openSettings = await _alertService.ShowAlertAsyncbool(
                    "تنبيه",
                    "يجب تفعيل GPS أولاً",
                    "فتح الإعدادات");

                if (openSettings)
                    AppInfo.ShowSettingsUI();

                return;
            }

            // 4️⃣ إدخال اسم الفرع
            string branchName = await DisplayPromptAsync(
                isEdit ? "تعديل الفرع" : "إضافة فرع جديد",
                "أدخل اسم الفرع:",
                initialValue: isEdit ? _selectedBranch?.Name : "",
                placeholder: "مثال: فرع القاهرة");

            if (string.IsNullOrWhiteSpace(branchName))
                return;

            // 5️⃣ إدخال المسافة المسموحة
            string radiusInput = await DisplayPromptAsync(
                "المسافة المسموحة",
                "أدخل المسافة المسموحة بالمتر:",
                initialValue: isEdit ? _selectedBranch?.RadiusMeters.ToString() : "100",
                keyboard: Keyboard.Numeric);

            if (!double.TryParse(radiusInput, out double radius) || radius <= 0)
            {
                await _alertService.ShowAlertAsync("خطأ", "المسافة غير صحيحة", "حسناً");
                return;
            }

            Loader.IsVisible = true;
            Loader.IsRunning = true;

            // 6️⃣ الحصول على معلومات الواي فاي
            string ssid = GetWifiSSID();
            string bssid = GetWifiBSSID();

            // 7️⃣ إعداد البيانات
            var data = new BranchItem
            {
                Id = isEdit ? _selectedBranch!.Id : 0,
                Name = branchName,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                RadiusMeters = radius,
                CompanyWifiSSID = ssid,
                CompanyWifiBSSID = bssid,
                RequireWifi = true,
                RequireGps = true,
                LastUpdated = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var token = await SecureStorage.Default.GetAsync("AccessToken");

            var client = HttpClientFactory.Instance;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/CompanyPrintApi/Add",
                content);

            if (response.IsSuccessStatusCode)
            {
                await _alertService.ShowAlertAsync(
                    "نجاح",
                    isEdit ? "تم تعديل الفرع بنجاح" : "تم إضافة الفرع بنجاح",
                    "حسنًا");

                await LoadBranches();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await _alertService.ShowAlertAsync("خطأ", $"فشل إرسال البيانات\n{errorContent}", "حسنًا");
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlertAsync("خطأ", ex.Message, "حسنًا");
        }
        finally
        {
            Loader.IsRunning = false;
            Loader.IsVisible = false;
        }
    }

    // =============================================
    // Helper Methods
    // =============================================
    public string GetWifiSSID()
    {
        var wifiManager = (WifiManager)Android.App.Application.Context
            .GetSystemService(Context.WifiService);

        var wifiInfo = wifiManager.ConnectionInfo;

        return wifiInfo?.SSID?.Replace("\"", "") ?? "غير معروف";
    }

    public string GetWifiBSSID()
    {
        var wifiManager = (WifiManager)Android.App.Application.Context
            .GetSystemService(Context.WifiService);

        var wifiInfo = wifiManager.ConnectionInfo;

        return wifiInfo?.BSSID ?? "غير معروف";
    }

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        await NavigationHelper.PopCurrentPageAsync();
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        NavigationHelper.ToggleFlyoutMenu();
    }

    protected override bool OnBackButtonPressed()
    {
        return NavigationHelper.HandleBackButton();
    }
}

// =============================================
// Model للفرع
// =============================================
public class BranchItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeters { get; set; }
    public string CompanyWifiSSID { get; set; } = string.Empty;
    public string CompanyWifiBSSID { get; set; } = string.Empty;
    public bool RequireWifi { get; set; }
    public bool RequireGps { get; set; }
    public DateTime LastUpdated { get; set; }
}