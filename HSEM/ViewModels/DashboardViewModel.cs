using HSEM.Interfaces;
using HSEM.Services;
using Microsoft.Maui.Storage;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly IPopupService _popup;

        readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("https://elnagarygroup-001-site1.ktempurl.com/api/DashboardApi/")
        };

        // =============================================
        //  Properties
        // =============================================
        string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                    OnPropertyChanged(nameof(NameInitials));
            }
        }

        // أول حرفين من الاسم للأفاتار
        public string NameInitials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_fullName)) return "?";
                var parts = _fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}";
                return _fullName.Length >= 2 ? _fullName[..2] : _fullName;
            }
        }

        bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        string _employeeCode;
        public string EmployeeCode { get => _employeeCode; set => SetProperty(ref _employeeCode, value); }

        string _jobTitle;
        public string JobTitle { get => _jobTitle; set => SetProperty(ref _jobTitle, value); }

        string _department;
        public string Department { get => _department; set => SetProperty(ref _department, value); }

        string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        string _picture;
        public string Picture { get => _picture; set => SetProperty(ref _picture, value); }

        string _workDuration;
        public string WorkDuration { get => _workDuration; set => SetProperty(ref _workDuration, value); }

        string _notes;
        public string Notes { get => _notes; set => SetProperty(ref _notes, value); }

        // --- الإحصائيات ---
        int _accepted;
        public int Accepted { get => _accepted; set => SetProperty(ref _accepted, value); }

        int _rejected;
        public int Rejected { get => _rejected; set => SetProperty(ref _rejected, value); }

        int _pending;
        public int Pending { get => _pending; set => SetProperty(ref _pending, value); }

        int _balance;
        public int Balance { get => _balance; set => SetProperty(ref _balance, value); }

        int _absenceDays;
        public int AbsenceDays { get => _absenceDays; set => SetProperty(ref _absenceDays, value); }

        int _todayAttendance;
        public int TodayAttendance { get => _todayAttendance; set => SetProperty(ref _todayAttendance, value); }

        string _absenceSummary;
        public string AbsenceSummary { get => _absenceSummary; set => SetProperty(ref _absenceSummary, value); }

        int _age;
        public int Age { get => _age; set => SetProperty(ref _age, value); }

        private DateTime? _hirdate;
        public DateTime? HirDate
        {
            get => _hirdate;
            set
            {
                if (SetProperty(ref _hirdate, value))
                    OnPropertyChanged(nameof(HirDateFormatted));
            }
        }
        public string HirDateFormatted => HirDate?.ToString("dd/MM/yyyy");

        private DateTime? _birthDate;
        public DateTime? BirthDate
        {
            get => _birthDate;
            set
            {
                if (SetProperty(ref _birthDate, value))
                    OnPropertyChanged(nameof(BirthDateFormatted));
            }
        }
        public string BirthDateFormatted => BirthDate?.ToString("dd/MM/yyyy");

        string? _qualification;
        public string? Qualification { get => _qualification; set => SetProperty(ref _qualification, value); }

        private LeaveSummaryModel _leaveSummary;
        public LeaveSummaryModel LeaveSummary
        {
            get => _leaveSummary;
            set => SetProperty(ref _leaveSummary, value);
        }

        // --- بيانات العرض ---
        public ObservableCollection<ChartData> MonthlyData { get; } = new();
        public ObservableCollection<PieData> LeaveDistribution { get; } = new();
        public ObservableCollection<PieData> AbsenceDistribution { get; } = new();
        public ObservableCollection<RequestItem> LatestRequests { get; } = new();

        bool _isRefreshing;
        public bool IsRefreshing { get => _isRefreshing; set => SetProperty(ref _isRefreshing, value); }

        public ICommand RefreshCommand => new Command(async () => await LoadDataAsync());

        // =============================================
        //  Constructor
        // =============================================
        public DashboardViewModel()
        {
            _popup = new PopupService();
            _ = LoadDataAsync();
        }

        // =============================================
        //  Load Data
        // =============================================
        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var userId = Preferences.Get("UserId", null);
                if (string.IsNullOrEmpty(userId))
                {
                     _popup.ShowSuccessToast( "لم يتم العثور على المستخدم.");
                    return;
                }

                IsRefreshing = true;

                var response = await _http.GetAsync($"GetDashboardData?userId={Uri.EscapeDataString(userId)}");
                if (!response.IsSuccessStatusCode)
                {
                     _popup.ShowSuccessToast( $"فشل تحميل البيانات: {response.StatusCode}");
                    return;
                }

                var jsonText = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(jsonText);

                // بيانات الموظف
                FullName = json.Value<string>("fullName") ?? "-";
                EmployeeCode = json.Value<string>("employeeCode") ?? "-";
                JobTitle = json.Value<string>("jobTitle") ?? "-";
                Department = json.Value<string>("department") ?? "-";
                Email = json.Value<string>("email") ?? "-";

                var pic = json.Value<string>("picture") ?? string.Empty;
                Picture = pic.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? pic
                    : (string.IsNullOrEmpty(pic)
                        ? string.Empty
                        : $"https://elnagarygroup-001-site1.ktempurl.com/Images/Employeespic/{pic}");

                WorkDuration = json.Value<string>("workDuration") ?? "-";
                Notes = json.Value<string>("notes") ?? "";

                // الإحصائيات
                Accepted = json.Value<int?>("accepted") ?? 0;
                Rejected = json.Value<int?>("rejected") ?? 0;
                Pending = json.Value<int?>("pending") ?? 0;
                Balance = json.Value<int?>("balance") ?? 0;
                AbsenceDays = json.Value<int?>("absenceDays") ?? 0;
                TodayAttendance = json.Value<int?>("todayAttendance") ?? 0;
                Age = json.Value<int?>("age") ?? 0;
                HirDate = json.Value<DateTime?>("hireDate");
                BirthDate = json.Value<DateTime?>("birthDate");
                Qualification = json.Value<string?>("qualification");

                // تعبئة نموذج الإجازات
                LeaveSummary = new LeaveSummaryModel
                {
                    TotalBalance = Balance,
                    Accepted = Accepted,
                    Rejected = Rejected,
                    Pending = Pending
                };

                // بيانات الرسم الشهري
                MonthlyData.Clear();
                var monthsArr = json["monthlyData"];
                if (monthsArr != null && monthsArr.Type == JTokenType.Array)
                {
                    var months = monthsArr.ToObject<int[]>();
                    string[] monthNames =
                    {
                        "يناير", "فبراير", "مارس", "إبريل", "مايو", "يونيو",
                        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
                    };
                    for (int i = 0; i < months.Length && i < monthNames.Length; i++)
                        MonthlyData.Add(new ChartData(monthNames[i], months[i]));
                }

                // توزيع الإجازات
                LeaveDistribution.Clear();
                LeaveDistribution.Add(new PieData("مقبولة", Accepted));
                LeaveDistribution.Add(new PieData("مرفوضة", Rejected));
                LeaveDistribution.Add(new PieData("قيد الانتظار", Pending));
                LeaveDistribution.Add(new PieData("الرصيد", Balance));

                // توزيع الغياب
                AbsenceDistribution.Clear();
                int presentDays = Math.Max(0, 365 - AbsenceDays);
                AbsenceDistribution.Add(new PieData("غياب", AbsenceDays));
                AbsenceDistribution.Add(new PieData("حضور", presentDays));
                AbsenceSummary = $"غياب: {AbsenceDays} يوم  •  حضور: {presentDays} يوم";

                // الطلبات الأخيرة
                var latest = json["latestRequests"];
                if (latest != null && latest.Type == JTokenType.Array)
                {
                    var tempList = new List<RequestItem>();
                    int i = 1;
                    foreach (var r in latest)
                    {
                        tempList.Add(new RequestItem
                        {
                            Index = i++,
                            Date = r.Value<string>("fromDate") ?? "-",
                            DaysCount = r["daysCount"]?.ToString() ?? "0",
                            Status = ConvertStatusToArabic(r.Value<string>("status"))
                        });
                    }

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LatestRequests.Clear();
                        foreach (var r in tempList)
                            LatestRequests.Add(r);
                    });
                }
            }
            catch (Exception ex)
            {
                // ✅ لو مفيش نت → رسالة واضحة للموظف
                if (ex.Message.Contains("Connection") || ex.Message.Contains("connect") ||
                    ex.Message.Contains("network") || ex.Message.Contains("Network") ||
                    ex.Message.Contains("internet") || ex.InnerException?.Message.Contains("Connection") == true)
                {
                     _popup.ShowSuccessToast(
                        "لا يوجد اتصال بالإنترنت 📴 \n"+
                        "لا يمكن تحميل البيانات الآن\n\n" +
                        "✅ يمكنك تسجيل الحضور أو الانصراف بدون إنترنت\n" +
                        "وسيتم إرسال البيانات تلقائياً عند عودة الاتصال"+
                        "تمام");
                }
                else
                {
                     _popup.ShowSuccessToast( ex.Message);
                }
            }
            finally
            {
                IsRefreshing = false;
                IsLoading = false;
            }
        }

        // =============================================
        //  Helpers
        // =============================================
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static string ConvertStatusToArabic(string? status) => status switch
        {
            "Approved" => "مقبولة",
            "Rejected" => "مرفوضة",
            "Pending" => "قيد الانتظار",
            _ => "غير معروف"
        };
    }

    // =============================================
    //  Supporting Models
    // =============================================
    public class LatestRequestItem
    {
        public string Date { get; set; }
        public string DaysCount { get; set; }
        public string Status { get; set; }
    }

    public class ChartData
    {
        public string Month { get; set; }
        public int Value { get; set; }
        public ChartData(string m, int v) { Month = m; Value = v; }
    }

    public class PieData
    {
        public string Category { get; set; }
        public double Value { get; set; }
        public PieData(string c, double v) { Category = c; Value = v; }
    }

    public class RequestItem
    {
        public int Index { get; set; }
        public string Date { get; set; }
        public string DaysCount { get; set; }
        public string Status { get; set; }
    }

    public class LeaveSummaryModel
    {
        public int TotalBalance { get; set; }
        public int Accepted { get; set; }
        public int Rejected { get; set; }
        public int Pending { get; set; }
    }
}