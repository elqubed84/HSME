using HSEM.Interfaces;
using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public class PermissionQueryService : IPermissionQueryService
    {
        private readonly HttpClient _http = HttpClientFactory.Instance;

        public async Task<List<MyPermissionRequestDto>> GetMyPermissionsAsync(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.GetAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/PermissionRequestApi/MyRequests");

            if (!response.IsSuccessStatusCode)
                throw new Exception("فشل تحميل طلبات التصاريح");

            var raw = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<MyPermissionRequestDto>>(raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<MyPermissionRequestDto>();
        }
    }

}
