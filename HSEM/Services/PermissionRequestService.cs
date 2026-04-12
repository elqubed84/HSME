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
    public class PermissionRequestService : IPermissionRequestService
    {
        private readonly HttpClient _http = new();

        public async Task<(bool success, string message)> SubmitAsync(PermissionRequestDto dto, string token)
        {
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _http.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _http.PostAsync(
                        "https://elnagarygroup-001-site1.ktempurl.com/api/PermissionRequestApi/Create", content);

                    var raw = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var parsed = JsonSerializer.Deserialize<ServerResponse>(raw,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return (response.IsSuccessStatusCode, parsed?.Message ?? raw);
                    }
                    catch
                    {
                        return (response.IsSuccessStatusCode, raw);
                    }
                }
                catch when (attempt < maxRetries)
                {
                    await Task.Delay(1000 * attempt); // backoff تدريجي
                }
            }

            return (false, "تعذر الاتصال بالخادم بعد عدة محاولات");
        }
    }

}
