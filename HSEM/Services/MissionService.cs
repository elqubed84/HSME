using System.Net.Http.Headers;
using System.Net.Http.Json;
using HSEM.Models;

namespace HSEM.Services
{
    public class MissionService : IMissionService
    {
        private readonly HttpClient _httpClient;

        public MissionService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://elnagarygroup-001-site1.ktempurl.com/api/")
            };
        }

        public async Task<(bool success, string message)> SubmitAsync(
            MissionRequestDto dto,
            string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response =
                await _httpClient.PostAsJsonAsync("MissionRequest/Create", dto);

            if (response.IsSuccessStatusCode)
                return (true, "تم إرسال طلب المأمورية بنجاح");

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        public async Task<List<MissionDto>> GetMyMissionsAsync(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("MissionRequest/MyMissions");

            if (!response.IsSuccessStatusCode)
                return new List<MissionDto>();

            return await response.Content.ReadFromJsonAsync<List<MissionDto>>();
        }
    }
}