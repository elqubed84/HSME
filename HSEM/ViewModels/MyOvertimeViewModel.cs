using HSEM.Interfaces;
using HSEM.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class CompensatoryDayVM
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("workDate")]
        public DateTime? WorkDate { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }
    public class MyOvertimeViewModel : BindableObject
    {
        private readonly IApiService _api;
        private readonly IPopupService _popup;
        public ObservableCollection<CompensatoryDayVM> Requests { get; set; } = new();
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand LoadRequestsCommand { get; }

        public MyOvertimeViewModel()
        {
            _api = new ApiService();
            _popup = new PopupService();
            LoadRequestsCommand = new Command(async () => await LoadRequests());
        }

        private async Task LoadRequests()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var accessToken = await SecureStorage.Default.GetAsync("AccessToken");
                var res = await _api.GetWithTokenAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/CompensatoryDays/myrequests",
                    accessToken
                );

                if (!res.IsSuccessStatusCode)
                {
                     _popup.ShowSuccessToast( $"Status: {res.StatusCode}");
                    return;
                }

                var json = await res.Content.ReadAsStringAsync();

                // مهم: ignore case عشان يحل مشكلة أسماء الحقول
                var arr = JsonSerializer.Deserialize<CompensatoryDayVM[]>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

                Requests.Clear();
                if (arr != null)
                {
                    foreach (var item in arr)
                    {
                        // لو أي تاريخ null خلي التاريخ اليوم بدلاً من 1/1/0001
                        if (item.WorkDate == null) item.WorkDate = DateTime.Today;
                        if (item.CreatedAt == null) item.CreatedAt = DateTime.Now;
                        if (string.IsNullOrEmpty(item.Description)) item.Description = "لا يوجد وصف";
                        if (string.IsNullOrEmpty(item.Status)) item.Status = "قيد المراجعة";


                        Requests.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                 _popup.ShowSuccessToast( ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

}
