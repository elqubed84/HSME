using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using HSEM.Interfaces;
using HSEM.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using HSEM.Services;

namespace HSEM.ViewModels
{
    public class MyLoanRequestsViewModel : BindableObject
    {
        private readonly IApiService _apiService;

        public ObservableCollection<MyLoanRequestDto> MyRequests { get; } = new();

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand LoadCommand { get; }

        #region Constructors

        public MyLoanRequestsViewModel()
        {
            _apiService = new ApiService();
            LoadCommand = new Command(async () => await LoadAsync());
        }

        public MyLoanRequestsViewModel(IApiService apiService)
        {
            _apiService = apiService;
            LoadCommand = new Command(async () => await LoadAsync());
        }

        #endregion

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var token = await SecureStorage.Default.GetAsync("AccessToken");
                var response = await _apiService.GetWithTokenAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/LoanRequests/MyRequests", token);

                if (!response.IsSuccessStatusCode) return;

                var raw = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<MyLoanRequestDto>>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data == null) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MyRequests.Clear();
                    foreach (var item in data) MyRequests.Add(item);
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
