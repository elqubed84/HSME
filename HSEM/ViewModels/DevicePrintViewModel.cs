using HSEM.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class DevicePrintViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cts;

        public event PropertyChangedEventHandler PropertyChanged;

        public DevicePrintViewModel()
        {

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://elnagarygroup-001-site1.ktempurl.com/"),
                Timeout = TimeSpan.FromSeconds(30)
            };

            FilteredPrints = new ObservableCollection<DevicePrintDto>();

            ResetCommand = new Command<int>(async (id) => await ResetFingerprint(id));
        }

        public async Task InitializeAsync()
        {
            var token = await SecureStorage.Default.GetAsync("AccessToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }


        // =============================
        // Properties
        // =============================

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    _ = PerformLiveSearch(_searchText);
                }
            }
        }

        private string _searchStatusMessage;
        public string SearchStatusMessage
        {
            get => _searchStatusMessage;
            set { _searchStatusMessage = value; OnPropertyChanged(); }
        }

        private bool _isStatusVisible;
        public bool IsStatusVisible
        {
            get => _isStatusVisible;
            set { _isStatusVisible = value; OnPropertyChanged(); }
        }

        private bool _isListVisible;
        public bool IsListVisible
        {
            get => _isListVisible;
            set { _isListVisible = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DevicePrintDto> _filteredPrints;
        public ObservableCollection<DevicePrintDto> FilteredPrints
        {
            get => _filteredPrints;
            set
            {
                _filteredPrints = value;
                OnPropertyChanged();
            }
        }

        public ICommand ResetCommand { get; }

        // =============================
        // Live Search
        // =============================
        private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(1500);
        private DateTime _lastKeystroke = DateTime.MinValue;

        private async Task PerformLiveSearch(string keyword)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _lastKeystroke = DateTime.Now;

            try
            {
                // انتظر فترة الديبونس أو إلغاء المهمة
                while ((DateTime.Now - _lastKeystroke) < _debounceTime)
                {
                    await Task.Delay(50, token);
                    if (token.IsCancellationRequested) return;
                }

                // لو السيرش فاضي، نرجع مباشرة
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    FilteredPrints.Clear();
                    SearchStatusMessage = "اكتب كود الموظف أو البريد الإلكتروني للبحث";
                    IsStatusVisible = true;
                    IsListVisible = false;
                    return;
                }

                IsLoading = true;

                string url = "api/DevicePrints/list";
                url += $"?search={Uri.EscapeDataString(keyword)}";

                var response = await _httpClient.GetAsync(url, token);
                if (!response.IsSuccessStatusCode)
                {
                    SearchStatusMessage = "حدث خطأ أثناء تحميل البيانات";
                    IsStatusVisible = true;
                    IsListVisible = false;
                    return;
                }

                var json = await response.Content.ReadAsStringAsync(token);
                var data = JsonConvert.DeserializeObject<List<DevicePrintDto>>(json);

                if (!token.IsCancellationRequested)
                {
                    //FilteredPrints = new ObservableCollection<DevicePrintDto>(data);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        FilteredPrints.Clear();
                    });

                    foreach (var item in data)
                    {
                        if (token.IsCancellationRequested) return;

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            FilteredPrints.Add(item);
                        });

                        await Task.Delay(1); // يدي فرصة للـ UI يرسم Frame
                    }


                    if (FilteredPrints.Count == 0)
                    {
                        SearchStatusMessage = "لا توجد نتائج مطابقة";
                        IsStatusVisible = true;
                        IsListVisible = false;
                    }
                    else
                    {
                        IsStatusVisible = false;
                        IsListVisible = true;
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                SearchStatusMessage = "حدث خطأ أثناء تحميل البيانات";
                IsStatusVisible = true;
                IsListVisible = false;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }


        // =============================
        // Reset Fingerprint
        // =============================
        private async Task ResetFingerprint(int id)
        {
            try
            {
                IsLoading = true;

                var response = await _httpClient.DeleteAsync($"api/DevicePrints/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var item = FilteredPrints.FirstOrDefault(x => x.Id == id);
                    if (item != null)
                    {
                        FilteredPrints.Remove(item);
                        SearchStatusMessage = "تم حذف البصمة بنجاح";
                        IsStatusVisible = true;

                        if (FilteredPrints.Count == 0)
                        {
                            IsListVisible = false;
                        }
                    }
                }
                else
                {
                    SearchStatusMessage = "حدث خطأ أثناء حذف البصمة";
                    IsStatusVisible = true;
                }
            }
            catch (Exception ex)
            {
                SearchStatusMessage = "حدث خطأ أثناء حذف البصمة";
                IsStatusVisible = true;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // =============================
        // Notify
        // =============================
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // =============================
    // DTO
    // =============================
    public class DevicePrintDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public string EmployeeCode { get; set; } // جديد
    }
}
