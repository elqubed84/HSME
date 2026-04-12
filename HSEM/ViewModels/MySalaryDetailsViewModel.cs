using HSEM.Interfaces;
using HSEM.Services;
using System.Net.Http.Headers;
using System.Text.Json;

public class MySalaryDetailsViewModel : BindableObject
{
    private readonly IPopupService _alertService;
    private readonly HttpClient _httpClient = new();

    public int Year { get; set; }
    public int MonthNumber { get; set; }
    public string MonthName { get; set; } = string.Empty;

    private MonthlyPayrollDto? _payroll;
    public MonthlyPayrollDto? Payroll
    {
        get => _payroll;
        set
        {
            _payroll = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalRemaining));
            OnPropertyChanged(nameof(TotalPenalties));
            OnPropertyChanged(nameof(TotalAllowances)); // ✅ جديد
        }
    }

    public decimal TotalRemaining => Payroll?.LoansList?.Sum(l => l.Remaining) ?? 0m;
    public decimal TotalPenalties => Payroll?.PenaltiesList?.Sum(p => p.DeductionValue) ?? 0m;
    public decimal TotalAllowances => Payroll?.AllowancesList?.Sum(a => a.Amount) ?? 0m; // ✅ جديد

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    private bool _isSalaryVisible;
    public bool IsSalaryVisible
    {
        get => _isSalaryVisible;
        set { _isSalaryVisible = value; OnPropertyChanged(); }
    }

    public string SelectedYearMonthText => $"مرتب شهر {MonthName} لسنة {Year}";

    public MySalaryDetailsViewModel(int year, int monthNumber, string monthName)
    {
        _alertService = new PopupService();
        Year = year;
        MonthNumber = monthNumber;
        MonthName = monthName;

        _ = LoadPayrollAsync();
    }

    private async Task LoadPayrollAsync()
    {
        try
        {
            IsBusy = true;
            IsSalaryVisible = false;

            var token = await SecureStorage.Default.GetAsync("AccessToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                await _alertService.ShowAlertAsync("خطأ", "انتهت صلاحية الجلسة", "موافق");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://elnagarygroup-001-site1.ktempurl.com/api/PayrollApi/{Year}/{MonthNumber}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                await _alertService.ShowAlertAsync("تنبيه", "لا يوجد بيانات مرتب", "موافق");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<MonthlyPayrollDto>(json, options);

            if (result != null)
            {
                Payroll = result;
                IsSalaryVisible = true;
            }
            else
            {
                await _alertService.ShowAlertAsync("تنبيه", "فشل قراءة البيانات", "موافق");
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowAlertAsync("خطأ", $"حدث خطأ أثناء تحميل البيانات: {ex.Message}", "موافق");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

// DTOs متوافقة مع API
public class MonthlyPayrollDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;

    public decimal BasicSalary { get; set; }
    public int AbsentDays { get; set; }
    public decimal AbsentDeduction { get; set; }
    public decimal BaseAfterAbsence { get; set; }

    public decimal Allowances { get; set; }
    public decimal ExtraHoursAmount { get; set; }
    public decimal Bonus { get; set; }

    public decimal Deductions { get; set; }
    public decimal Loans { get; set; }
    public decimal InsuranceEmployee { get; set; }
    public decimal InsuranceCompany { get; set; }
    public decimal IncomeTax { get; set; }

    public decimal NetSalary { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<LoanDto> LoansList { get; set; } = new();
    public List<PenaltyDto> PenaltiesList { get; set; } = new();
    public List<AllowanceDto> AllowancesList { get; set; } = new(); // ✅ جديد
}

public class LoanDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal Remaining { get; set; }
    public int Installments { get; set; }
    public int PaidInstallments { get; set; }
    public int RemainingInstallments { get; set; }
}

public class PenaltyDto
{
    public string Description { get; set; } = string.Empty;
    public decimal DeductionValue { get; set; }
    public DateTime Date { get; set; }
}

// ✅ DTO جديد للبدلات
public class AllowanceDto
{
    public string AllowanceTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CalculationType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCustom { get; set; }
    public string? Notes { get; set; }
}