using HSEM.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.ViewModels
{
    public class DisciplinaryViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient = HttpClientFactory.Instance;

        public ObservableCollection<DisciplinaryDto> Actions { get; set; } = new();

        public async Task Load()
        {
            var token = await SecureStorage.Default.GetAsync("AccessToken");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var result = await _httpClient.GetFromJsonAsync<DisciplinaryResponse>(
                "https://elnagarygroup-001-site1.ktempurl.com/api/MobileHr/disciplinary");

            Actions.Clear();

            foreach (var item in result.actions)
                Actions.Add(item);
        }
    }
    public class DisciplinaryDto
    {
        public int id { get; set; }
        public string level { get; set; }
        public int levelCode { get; set; }
        public string reason { get; set; }
        public string actionDate { get; set; }
        public string expiryDate { get; set; }
        public bool isExpired { get; set; }
    }

    public class DisciplinaryResponse
    {
        public int count { get; set; }
        public List<DisciplinaryDto> actions { get; set; }
    }
}
