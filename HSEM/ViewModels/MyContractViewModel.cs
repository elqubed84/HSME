using HSEM.Models;
using HSEM.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class MyContractViewModel : BaseViewModel
    {
        public ObservableCollection<EmployeeContractModel> Contracts { get; set; } = new();
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        public ICommand LoadCommand { get; }

        private readonly HttpClient _httpClient;

        public MyContractViewModel()
        {
            _httpClient = HttpClientFactory.Instance; // لو عندك API Base URL ضيفه
            LoadCommand = new Command(async () => await LoadContractsAsync());
            IsLoading = false; // ← تأكد إنها false عند البداية
        }

        public async Task LoadContractsAsync()
        {
            //if (IsLoading) return;
            IsLoading = true;

            try
            {
                // ← جلب التوكن من التخزين الآمن
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                // ← طلب الـ API
                var response = await _httpClient.GetFromJsonAsync<MyContractResponse>(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/MobileHr/contract");

                Contracts.Clear();

                if (response != null && response.hasContract)
                {
                    Contracts.Add(new EmployeeContractModel
                    {
                        Id = response.id,
                        ContractType = response.contractType,
                        StartDate = DateTime.Parse(response.startDate),
                        EndDate = string.IsNullOrEmpty(response.endDate) ? null : DateTime.Parse(response.endDate),
                        Status = response.isPermanent ? "دائم" : (response.isExpiringSoon ? "على وشك الانتهاء" : "ساري"),
                        Notes = response.notes ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                // ممكن تحط هنا لوج للأخطاء
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

        public class EmployeeContractModel
        {
            public int Id { get; set; }
            public string ContractType { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;

            public int? DaysLeft
            {
                get
                {
                    if (!EndDate.HasValue) return null;
                    return (int?)(EndDate.Value.Date - DateTime.Today).TotalDays;
                }
            }

            public bool IsExpiringSoon => DaysLeft.HasValue && DaysLeft <= 30;
            public bool IsPermanent => !EndDate.HasValue;
        }
    
    public class MyContractResponse
    {
        public bool hasContract { get; set; }
        public int id { get; set; }
        public string contractType { get; set; } = string.Empty;
        public string startDate { get; set; } = string.Empty;
        public string? endDate { get; set; }
        public bool isPermanent { get; set; }
        public int? daysLeft { get; set; }
        public bool isExpiringSoon { get; set; }
        public string? notes { get; set; }
    }
}