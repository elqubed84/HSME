using HSEM.Interfaces;
using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public class AttendancePolicyService : IAttendancePolicyService
    {
        private readonly HttpClient _httpClient;
        private const string POLICY_CACHE_KEY = "LAST_ATT_POLICY";

        public AttendancePolicyService()
        {
            _httpClient = HttpClientFactory.Instance;
        }


public async Task<AttendancePolicy> GetPolicyAsync()
    {
        try
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                var response = await _httpClient.GetAsync("attendance/policy");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                await SecureStorage.SetAsync(POLICY_CACHE_KEY, json);

                return JsonSerializer.Deserialize<AttendancePolicy>(json)!;
            }
        }
        catch
        {
            // fallback
        }

        var cached = await SecureStorage.GetAsync(POLICY_CACHE_KEY);
        if (string.IsNullOrWhiteSpace(cached))
            return null!; // هنتعامل معاها فوق

        return JsonSerializer.Deserialize<AttendancePolicy>(cached)!;
    }
}
}
