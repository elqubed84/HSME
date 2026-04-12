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
    public class MyPermissionsViewModel : BindableObject
    {
        private readonly IApiService _apiService;
        private readonly IPopupService _alert;

        public ObservableCollection<MyPermissionRequestDto> MyRequests { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }

        #region Constructors

        public MyPermissionsViewModel()
        {
            _apiService = new ApiService();
            _alert = new PopupService();
            LoadCommand = new Command(async () => await LoadAsync());
        }

        public MyPermissionsViewModel(IApiService apiService, IPopupService alert)
        {
            _apiService = apiService;
            _alert = alert;
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
                    "https://elnagarygroup-001-site1.ktempurl.com/api/PermissionRequestApi/MyRequests", token);

                if (!response.IsSuccessStatusCode)
                {
                    await _alert.ShowAlertAsync("خطأ", "فشل تحميل الطلبات", "موافق");
                    return;
                }

                var raw = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ObservableCollection<MyPermissionRequestDto>>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data == null) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MyRequests.Clear();
                    foreach (var item in data)
                        MyRequests.Add(item);
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
