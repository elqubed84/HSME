using HSEM.Interfaces;
using HSEM.Models;
using HSEM.Services;
using HSEM.Views;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class MyEvaluationsViewModel : BindableObject
    {
        private readonly IApiService _apiService;

        public ObservableCollection<MyEvaluationDto> Evaluations { get; } = new();
        public ObservableCollection<int> Years { get; } = new();

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged();
                   // MainThread.BeginInvokeOnMainThread(async () => await LoadAsync());
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private string _noEvaluationsMessage = string.Empty;
        public string NoEvaluationsMessage
        {
            get => _noEvaluationsMessage;
            set { _noEvaluationsMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }
        public ICommand ItemSelectedCommand { get; }

        public MyEvaluationsViewModel()
        {
            _apiService = new ApiService();

            LoadCommand = new Command(async () => await LoadAsync());

            ItemSelectedCommand = new Command<MyEvaluationDto>(async selected =>
            {
                if (selected == null)
                    return;

                var page = new EmployeeEvaluationDetailsPage(selected.Month, selected.Year);

                if (Application.Current?.MainPage is FlyoutPage flyout)
                {
                    if (flyout.Detail is NavigationPage nav)
                        await nav.PushAsync(page);
                    else
                        await flyout.Detail.Navigation.PushAsync(page);
                }
                else
                {
                    await Application.Current.MainPage.Navigation.PushAsync(page);
                }
            });


            // تحضير السنوات
            var currentYear = DateTime.Now.Year;
            for (int i = 0; i < 5; i++)
                Years.Add(currentYear - i);

            SelectedYear = currentYear;
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // نظف القائمة على الـ UI thread
                await MainThread.InvokeOnMainThreadAsync(() => Evaluations.Clear());
                NoEvaluationsMessage = string.Empty;

                var token = await SecureStorage.Default.GetAsync("AccessToken");
                var url = $"https://elnagarygroup-001-site1.ktempurl.com/api/Evaluations/evaluations?year={SelectedYear}";

                var response = await _apiService.GetWithTokenAsync(url, token);
                if (!response.IsSuccessStatusCode)
                {
                    NoEvaluationsMessage = "فشل تحميل التقييمات";
                    return;
                }

                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw) || raw == "null")
                {
                    NoEvaluationsMessage = "لا توجد تقييمات لهذه السنة";
                    return;
                }

                List<MyEvaluationDto>? data = null;

                // محاولة deserialize مباشرة أولاً (الأسهل)
                try
                {
                    data = JsonSerializer.Deserialize<List<MyEvaluationDto>>(raw,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
                catch (System.Text.Json.JsonException)
                {
                    // إذا فشل، نحاول قراءة JSON ديناميكياً ونعيد بناء الـ list لتجنُّب مشاكل النوع
                    try
                    {
                        using var doc = JsonDocument.Parse(raw);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            var tmp = new List<MyEvaluationDto>();
                            foreach (var el in doc.RootElement.EnumerateArray())
                            {
                                var month = el.TryGetProperty("month", out var pMonth) && pMonth.TryGetInt32(out var m) ? m : 0;
                                var year = el.TryGetProperty("year", out var pYear) && pYear.TryGetInt32(out var y) ? y : SelectedYear;
                                // score قد جاء كـ number أو string -> حاول نقرأه بمرونة
                                double score = 0;
                                if (el.TryGetProperty("score", out var pScore))
                                {
                                    if (pScore.ValueKind == JsonValueKind.Number && pScore.TryGetDouble(out var sd))
                                        score = sd;
                                    else if (pScore.ValueKind == JsonValueKind.String && double.TryParse(pScore.GetString(), out var sd2))
                                        score = sd2;
                                }

                                var notes = el.TryGetProperty("notes", out var pNotes) && pNotes.ValueKind != JsonValueKind.Null
                                    ? pNotes.GetString()
                                    : null;

                                var monthText = el.TryGetProperty("monthText", out var pText) && pText.ValueKind != JsonValueKind.Null
                                    ? pText.GetString() ?? string.Empty
                                    : new DateTime(year, Math.Max(1, month), 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("ar-EG"));

                                tmp.Add(new MyEvaluationDto
                                {
                                    Month = month,
                                    Year = year,
                                    Score = score,
                                    Notes = notes,
                                    MonthText = monthText
                                });
                            }

                            data = tmp;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON fallback parse failed: {ex}");
                        NoEvaluationsMessage = "تعذّر قراءة بيانات السيرفر";
                        return;
                    }
                }

                if (data == null || data.Count == 0)
                {
                    NoEvaluationsMessage = "لا توجد تقييمات لهذه السنة";
                    return;
                }

                // احتفظ فقط بالعناصر التي لديها Score >= 0 (أنت تريد الأشهر التي فيها تقييم — حسب شرطك Score>0)
                var filtered = data.Where(x => x.Score > 0).ToList();

                if (!filtered.Any())
                {
                    NoEvaluationsMessage = "لا توجد تقييمات لهذه السنة";
                    return;
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var item in filtered)
                        Evaluations.Add(item);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadAsync Exception: " + ex);
                NoEvaluationsMessage = "حدث خطأ أثناء تحميل التقييمات";
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
