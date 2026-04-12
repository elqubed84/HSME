using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HSEM.Interfaces;
using HSEM.Services;
using HSEM.Views;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.ViewModels;

public class ChangePasswordViewModel : ObservableObject
{
    private readonly IApiService _api;
    private readonly IPopupService _popup;

    public ChangePasswordViewModel()
    {
        _api = new ApiService();
        _popup = new PopupService();

        ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
    }

    private string _currentPassword;
    public string CurrentPassword { get => _currentPassword; set => SetProperty(ref _currentPassword, value); }

    private string _newPassword;
    public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

    private string _confirmPassword;
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

    private bool _isBusy;
    public bool IsNotBusy => !_isBusy;

    public IAsyncRelayCommand ChangePasswordCommand { get; }

    private async Task ChangePasswordAsync()
    {
        if (_isBusy) return;
        _isBusy = true;
        OnPropertyChanged(nameof(IsNotBusy));

        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await _popup.ShowAlertAsync("تنبيه", "جميع الحقول مطلوبة.", "موافق");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                 _popup.ShowSuccessToast( "كلمة المرور الجديدة وتأكيدها غير متطابقين.");
                return;
            }

            // جلب التوكن من SecureStorage
            var token = await SecureStorage.Default.GetAsync("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                 _popup.ShowSuccessToast( "لم يتم تسجيل الدخول.");
                return;
            }

            var payload = new
            {
                CurrentPassword,
                NewPassword
            };

            var request = new HttpRequestMessage(HttpMethod.Post,"https://elnagarygroup-001-site1.ktempurl.com/api/ChangePassword/Change")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await HttpClientFactory.Instance.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                try
                {
                    var obj = JsonSerializer.Deserialize<JsonElement>(content);
                    var message = obj.TryGetProperty("message", out var msg) ? msg.GetString() : content;
                     
                    await _popup.ShowAlertAsync("تنبيه", message, "تمام");
                }
                catch
                {
                    // لو مش JSON
                     _popup.ShowSuccessToast( content);
                }

                return;
            }
            await _popup.ShowAlertAsync("تنبيه", "تم تغيير كلمة المرور بنجاح.", "سيتم تسجيل خروجك الآن.");

            Preferences.Clear();
            SecureStorage.Default.RemoveAll();
            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // ✅ التحقق من MainPage
                    if (App.Current?.MainPage == null)
                    {
                        App.Current.MainPage = new AppFlyoutPage();
                    }

                    // ✅ محاولة Navigation عادي
                    var flyoutPage = App.Current.MainPage as FlyoutPage;

                    if (flyoutPage != null)
                    {
                        // ✅ في FlyoutPage → استخدمه
                        flyoutPage.Detail = new NavigationPage(new LoginPage());
                        flyoutPage.IsPresented = false;
                    }
                    else
                    {
                        // ⚠️ مفيش FlyoutPage → أعد إنشاءه
                        App.Current.MainPage = new FlyoutPage
                        {
                            Flyout = new AppFlyoutPageFlyout(),
                            Detail = new NavigationPage(new LoginPage())
                        };
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Navigation error after password change: {ex.Message}");

                // ✅ آخر محاولة
                App.Current.MainPage = new AppFlyoutPage
                {
                    Detail = new NavigationPage(new LoginPage())
                };
            }

            CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
        }
        catch (HttpRequestException ex)
        {
            await _popup.ShowAlertAsync("خطأ", "خطأ اتصال .\n " + ex.Message, "تمام");

        }
        catch (Exception ex)
        {
            await _popup.ShowAlertAsync("خطأ",  ex.Message, "تمام");
        }
        finally
        {
            _isBusy = false;
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

}
