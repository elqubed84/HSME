using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using HSEM.Interfaces;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using HSEM.Models;
using HSEM.Services;

namespace HSEM.ViewModels
{
    public class MyRequestViewModel : BindableObject
    {
        private readonly IApiService _apiService;

        public ObservableCollection<MyLeaveRequestDto> MyRequests { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }

        #region Constructors

        public MyRequestViewModel()
        {
            _apiService = new ApiService();
            LoadCommand = new Command(async () => await LoadAsync());
        }

        public MyRequestViewModel(IApiService apiService)
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
                    "https://elnagarygroup-001-site1.ktempurl.com/api/MyLeaveRequest/MyList", token);

                if (!response.IsSuccessStatusCode) return;

                var raw = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<MyLeaveRequestDto>>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data == null) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MyRequests.Clear();
                    foreach (var item in data)
                    {
                        item.StartDate = DateTime.SpecifyKind(item.StartDate, DateTimeKind.Utc).ToLocalTime();
                        item.EndDate = DateTime.SpecifyKind(item.EndDate, DateTimeKind.Utc).ToLocalTime();
                        item.CreatedAt = DateTime.SpecifyKind(item.CreatedAt, DateTimeKind.Utc).ToLocalTime();

                        MyRequests.Add(item);
                    }
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
