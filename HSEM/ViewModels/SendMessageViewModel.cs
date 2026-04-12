using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using HSEM.Interfaces;
using HSEM.Services;

namespace HSEM.ViewModels
{
    public class SendMessageViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient = HttpClientFactory.Instance;
        private readonly IPopupService _popup;

        public ObservableCollection<EmployeeDto> Employees { get; set; } = new();

        private EmployeeDto _selectedEmployee;
        public EmployeeDto SelectedEmployee
        {
            get => _selectedEmployee;
            set => SetProperty(ref _selectedEmployee, value);
        }

        public string Subject { get; set; }
        public string Body { get; set; }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _showSuggestions;
        public bool ShowSuggestions
        {
            get => _showSuggestions;
            set => SetProperty(ref _showSuggestions, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand SendCommand { get; }

        public SendMessageViewModel()
        {
            _popup = new PopupService();
            SearchCommand = new Command<string>(async (q) => await Search(q));
            SendCommand = new Command(async () => await Send());
        }

        CancellationTokenSource _cts;

        private async Task Search(string q)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                IsSearching = true;
                await Task.Delay(400, _cts.Token);

                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    Employees.Clear();
                    ShowSuggestions = false; // اخفاء الاقتراحات
                    return;
                }

                if (q.All(char.IsDigit) && q.Length < 3)
                {
                    return;
                }

                var result = await _httpClient.GetFromJsonAsync<List<EmployeeDto>>(
                    $"https://elnagarygroup-001-site1.ktempurl.com/Admin/SearchEmployees?q={q}");

                Employees.Clear();
                if (result != null && result.Count > 0)
                {
                    foreach (var emp in result)
                        Employees.Add(emp);

                    ShowSuggestions = true; // إظهار الاقتراحات
                }
                else
                {
                    ShowSuggestions = false; // اخفاء اذا مفيش نتائج
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                 _popup.ShowSuccessToast( "فشل البحث: " + ex.Message);
            }
            finally
            {
                IsSearching = false;
            }
        }

        public void EmployeeSelected()
        {
            // اخفاء الاقتراحات بعد اختيار الموظف
            ShowSuggestions = false;
        }

        private async Task Send()
        {
            if (SelectedEmployee == null)
            {
                 _popup.ShowSuccessToast( "من فضلك اختر موظف للإرسال إليه.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Body))
            {
                 _popup.ShowSuccessToast( "الرجاء ملء الموضوع ونص الرسالة.");
                return;
            }

            try
            {
                IsBusy = true;
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var dto = new
                {
                    receiverId = SelectedEmployee.id,
                    subject = Subject.Trim(),
                    body = Body.Trim()
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/MobileHr/messages/compose", dto);

                if (response.IsSuccessStatusCode)
                {
                     _popup.ShowSuccessToast( "تم إرسال الرسالة بنجاح ✅");
                    Subject = string.Empty;
                    Body = string.Empty;
                    SelectedEmployee = null;
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                     _popup.ShowSuccessToast($"حصل خطأ: {response.StatusCode}\n{content}");
                }
            }
            catch (HttpRequestException ex)
            {
                 _popup.ShowSuccessToast("فشل الاتصال بالسيرفر ❌\n" + ex.Message);
            }
            catch (Exception ex)
            {
                 _popup.ShowSuccessToast( ex.Message);
            }
            finally { IsBusy = false; }
        }
    }

    public class EmployeeDto
    {
        public string id { get; set; }
        public string fullName { get; set; }
        public string employeeCode { get; set; }
        public string email { get; set; }
        public string department { get; set; }
        public string jobTitle { get; set; }
        public string picture { get; set; }
    }
}