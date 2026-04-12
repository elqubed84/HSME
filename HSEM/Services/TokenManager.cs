using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public static class TokenManager
    {
        private static readonly SemaphoreSlim _refreshLock = new(1, 1);

        public static async Task EnsureAccessTokenValidAsync()
        {
            var expiration = Preferences.Get("AccessTokenExpiration", DateTime.MinValue);
            if (DateTime.UtcNow < expiration)
                return; // التوكن لسه صالح

            // ✅ Lock عشان thread واحد بس يعمل refresh
            await _refreshLock.WaitAsync();
            try
            {
                // تحقق تاني بعد ما أخد الـ lock (ممكن thread تاني يكون عمل refresh)
                expiration = Preferences.Get("AccessTokenExpiration", DateTime.MinValue);
                if (DateTime.UtcNow < expiration)
                    return;

                var refreshToken = await SecureStorage.Default.GetAsync("RefreshToken") ?? "";
                if (string.IsNullOrEmpty(refreshToken))
                    throw new Exception("Refresh token not available");

                var request = new HttpRequestMessage(HttpMethod.Post,
                    "https://elnagarygroup-001-site1.ktempurl.com/api/auth/refresh")
                {
                    Content = System.Net.Http.Json.JsonContent.Create(refreshToken)
                };

                var response = await HttpClientFactory.Instance.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(resultString);
                    var result = doc.RootElement;

                    string newToken = result.GetProperty("accessToken").GetString() ?? "";
                    DateTime newExpiry = result.GetProperty("accessTokenExpiration").GetDateTime();

                    await SecureStorage.Default.SetAsync("AccessToken", newToken);
                    Preferences.Set("AccessTokenExpiration", newExpiry);
                }
                else
                {
                    throw new Exception("Unable to refresh token");
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }
    }

}
