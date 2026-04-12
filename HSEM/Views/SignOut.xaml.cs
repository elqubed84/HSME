#if ANDROID
using Android.Content;
#endif
using HSEM.Interfaces;
using HSEM.Platforms.Android.Services;
using HSEM.Services;

namespace HSEM.Views;

public partial class SignOut : ContentPage
{
    private readonly IPopupService _alertService;
    public SignOut()
	{
		InitializeComponent();
        _alertService = new PopupService();
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        lOADER.IsVisible = true;
        bool confirm = await DisplayAlert("تسجيل الخروج",
                                          "هل أنت متأكد أنك تريد تسجيل الخروج؟",
                                          "نعم",
                                          "إلغاء");

        if (!confirm)
        {
            lOADER.IsVisible = false;
        return;
        }

        await PerformLogoutAsync();
    }
    private async Task PerformLogoutAsync()
    {
        try
        {
            // مسح Preferences بالكامل
            Preferences.Clear();

            // مسح SecureStorage بالكامل
            SecureStorage.Default.RemoveAll();
#if ANDROID
            StopLocationService();
#endif

            // لو عندك Attendance Service شغال
            // AttendanceServiceHelper.StopAttendanceService();


            // إعادة تعيين الصفحة الرئيسية
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", ex.Message, "موافق");
        }
    }
#if ANDROID
    private static void StopLocationService()
    {
        try
        {
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(LocationForegroundService));
            intent.SetAction("ACTION_STOP_LOCATION_SERVICE");
            context.StartService(intent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Service stop error: {ex.Message}");
        }
    }
#endif

}