using HSEM.Models;
using HSEM.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HSEM.ViewModels
{
    public class MyAssetsViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient = HttpClientFactory.Instance;

        public ObservableCollection<AssetDto> Assets { get; set; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadCommand { get; }

        public MyAssetsViewModel()
        {
            LoadCommand = new Command(async () => await LoadAssets());
        }

        public async Task LoadAssets()
        {
            try
            {
                IsLoading = true;

                var token = await SecureStorage.Default.GetAsync("AccessToken");

                if (string.IsNullOrEmpty(token))
                    return;

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var result = await _httpClient.GetFromJsonAsync<AssetsResponse>(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/MobileHr/assets");

                Assets.Clear();

                if (result?.Assets != null)
                {
                    foreach (var item in result.Assets)
                        Assets.Add(item);
                }
            }
            catch (Exception ex)
            {
                // logging
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
