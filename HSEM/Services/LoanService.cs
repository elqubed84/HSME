using HSEM.Interfaces;
using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HSEM.Services
{
    public class LoanService : ILoanService
    {
        private readonly HttpClient _http;

        public LoanService()
        {
            _http = HttpClientFactory.Instance;
        }

        public async Task<(bool success, string message)> SubmitAsync(LoanRequestDto request, string token)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.PostAsync(
                "https://elnagarygroup-001-site1.ktempurl.com/api/LoanRequests/Create", content);

            var text = await response.Content.ReadAsStringAsync();

            try
            {
                var parsed = JsonSerializer.Deserialize<ServerResponse>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (response.IsSuccessStatusCode, parsed?.Message ?? text);
            }
            catch
            {
                return (response.IsSuccessStatusCode, text);
            }
        }
    }
}
