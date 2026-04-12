using HSEM.Interfaces;
using HSEM.Models;
using HSEM.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
namespace HSEM.ViewModels
{
    public class MyAttendanceDetailsViewModel : BindableObject
    {
        private readonly IPopupService _alertService;
        private readonly HttpClient _client;

        public MyAttendanceDetailsViewModel()
        {
            _alertService = new PopupService();
            _client = new HttpClient();
        }

        #region Properties

        private int _year;
        public int Year
        {
            get => _year;
            set
            {
                if (_year != value)
                {
                    _year = value;
                    OnPropertyChanged();
                    UpdateTitle();
                }
            }
        }

        private int _month;
        public int Month
        {
            get => _month;
            set
            {
                if (_month != value)
                {
                    _month = value;
                    OnPropertyChanged();
                    UpdateTitle();
                }
            }
        }

        private string _pageTitle = string.Empty;
        public string PageTitle
        {
            get => _pageTitle;
            set { _pageTitle = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AttendanceDayVM> AttendanceDays { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        #endregion

        private void UpdateTitle()
        {
            if (Year > 0 && Month > 0)
            {
                var monthName = new DateTime(Year, Month, 1)
                    .ToString("MMMM", new CultureInfo("ar-EG"));

                PageTitle = $"مؤثرات شهر {monthName} لسنة {Year}";
            }
        }

        public async Task LoadAsync()
        {
            if (Year == 0 || Month == 0)
                return;

            try
            {
                IsLoading = true;
                AttendanceDays.Clear();
                UpdateTitle(); // نضمن إعادة العنوان بدون إضافات قديمة

                var token = await SecureStorage.Default.GetAsync("AccessToken");
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url =
                    $"https://elnagarygroup-001-site1.ktempurl.com/api/MyAttenndance/MyList?year={Year}&month={Month}";

                var response =
                    await _client.GetFromJsonAsync<AttendanceResponseDto>(url);

                if (response == null || !response.HasData || response.Data.Count == 0)
                {
                    await _alertService.ShowAlertAsync(
                        "تنبيه",
                        response?.Message ?? "لا توجد مؤثرات لهذا الشهر",
                        "موافق");

                    return;
                }

                // نحول البيانات إلى Dictionary لتحسين الأداء
                var apiDays = response.Data
                    .ToDictionary(x => x.Date.Date);

                int daysInMonth = DateTime.DaysInMonth(Year, Month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(Year, Month, day);

                    if (apiDays.TryGetValue(date.Date, out var apiDay))
                    {
                        AttendanceDays.Add(apiDay);
                    }
                    else
                    {
                        AttendanceDays.Add(new AttendanceDayVM
                        {
                            Date = date,
                            IsAbsent = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync(
                    "خطأ",
                    $"حدث خطأ أثناء تحميل البيانات: {ex.Message}",
                    "موافق");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}