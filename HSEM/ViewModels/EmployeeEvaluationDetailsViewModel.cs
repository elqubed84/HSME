using HSEM.Interfaces;
using HSEM.Models;
using HSEM.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace HSEM.ViewModels;

public class EmployeeEvaluationDetailsViewModel : BindableObject
{
    private readonly int _month;
    private readonly int _year;
    private readonly IApiService _apiService;
    private bool _isBusy;

    public ObservableCollection<EmployeeEvaluationItemDto> Items { get; } = new();
    
    private decimal _averageScore = 0;
    public decimal AverageScore
    {
        get => _averageScore;
        set { _averageScore = value; OnPropertyChanged(); }
    }

    private string _notes = "لا توجد ملاحظات";
    public string Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public ICommand LoadCommand { get; }

    public EmployeeEvaluationDetailsViewModel(int month, int year, IApiService? apiService = null)
    {
        _month = month;
        _year = year;
        _apiService = apiService ?? new ApiService();
        LoadCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var token = await SecureStorage.Default.GetAsync("AccessToken");
            var url = $"https://elnagarygroup-001-site1.ktempurl.com/api/Evaluations/details?month={_month}&year={_year}";

            var response = await _apiService.GetWithTokenAsync(url, token);
            if (!response.IsSuccessStatusCode) return;

            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw)) return;

            List<EmployeeEvaluationItemDto> items = null;
            EvaluationDetailsDto result = null;
            try
            {
                result = JsonSerializer.Deserialize<EvaluationDetailsDto>(raw,
       new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Deserialize Error",
                    ex.ToString(),
                    "OK");

                await Application.Current.MainPage.DisplayAlert(
                    "RAW JSON",
                    raw,
                    "OK");
            }


            if (result == null)
                return;

            Items.Clear();

            if (result.Items != null)
            {
                foreach (var item in result.Items)
                    Items.Add(item);
            }



            // حساب المتوسط
            AverageScore = result.AverageScore;
            Notes = result.Notes ?? "لا توجد ملاحظات";

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("LoadAsync Exception: " + ex);
            await Application.Current.MainPage.DisplayAlert(
                   "Deserialize Error",
                   ex.Message.ToString(),
                   "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
