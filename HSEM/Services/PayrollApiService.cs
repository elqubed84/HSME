using HSEM.Interfaces;
using HSEM.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public class PayrollApiService
    {
        private readonly IApiService _apiService;

        public PayrollApiService()
        {
            _apiService = new ApiService();
        }

        public async Task<MonthlyPayroll?> GetPayrollAsync(int year, int month)
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                var url = $"https://elnagarygroup-001-site1.ktempurl.com/api/PayrollApi/{year}/{month}";
                var response = await _apiService.GetWithTokenAsync(url, token);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                // Debug: طبع JSON للتأكد من البيانات
                System.Diagnostics.Debug.WriteLine(json);

                return JsonSerializer.Deserialize<MonthlyPayroll>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }
    }
}
