using HSEM.Interfaces;
using Microsoft.Maui.Storage;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient = HttpClientFactory.Instance;

        public async Task EnsureAccessTokenValidAsync()
        {
            var expiration = Preferences.Get("AccessTokenExpiration", DateTime.MinValue);
            if (DateTime.UtcNow >= expiration)
            {
                // ✅ استخدم SecureStorage بدل Preferences للتوكن
                var refreshToken = await SecureStorage.Default.GetAsync("RefreshToken") ?? "";
                if (string.IsNullOrEmpty(refreshToken))
                    throw new Exception("Refresh token not available");

                var request = new HttpRequestMessage(HttpMethod.Post,
                    "https://elnagarygroup-001-site1.ktempurl.com/api/auth/refresh")
                {
                    Content = System.Net.Http.Json.JsonContent.Create(new { refreshToken })
                };

                var response = await HttpClientFactory.Instance.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Unable to refresh token");

                var resultString = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(resultString);
                var result = doc.RootElement;

                string newAccessToken = result.GetProperty("accessToken").GetString();
                DateTime newExpiration = result.GetProperty("accessTokenExpiration").GetDateTime();

                // ✅ SecureStorage للتوكن
                await SecureStorage.Default.SetAsync("AccessToken", newAccessToken);
                Preferences.Set("AccessTokenExpiration", newExpiration); // التاريخ عادي
            }
        }

        public async Task<HttpResponseMessage> GetWithTokenAsync(string url, string token = null)
        {
            await EnsureAccessTokenValidAsync();
            string accessToken = token ?? await SecureStorage.Default.GetAsync("AccessToken");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            return await _httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostWithTokenAsync(string url, HttpContent content, string token = null)
        {
            await EnsureAccessTokenValidAsync();
            string accessToken = token ?? await SecureStorage.Default.GetAsync("AccessToken");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            return await _httpClient.PostAsync(url, content);
        }
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await GetWithTokenAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await PostWithTokenAsync(url, content);
        }

    }
}
