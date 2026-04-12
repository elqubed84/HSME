using HSEM.Interfaces;
using HSEM.Services;
using HSEM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class AttendanceService : IAttendanceService
    {
        private readonly HttpClient _httpClient;
        private readonly IAttendancePolicyService _policyService;
        private bool _isProcessing;

        public AttendanceService()
        {
            _httpClient =  HttpClientFactory.Instance;
            _policyService = new AttendancePolicyService();
        }

        // ✅ تنفيذ VerifyOnlineAsync
        public async Task<bool> VerifyOnlineAsync(double lat, double lng, string ssid, string bssid, bool isMock, string action)
        {
            try
            {
                var payload = new
                {
                    Latitude = lat,
                    Longitude = lng,
                    SSID = ssid,
                    BSSID = bssid,
                    IsMockLocation = isMock,
                    Action = action
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // استخدام الدالة القديمة اللي فيها التوكن
                var response = await LoginPage.PostWithTokenAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/AttendancePolicy/verify",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyOfflineAsync(OfflineAttendance attendance)
        {
            try
            {
                var payload = new
                {
                    Latitude = attendance.Latitude,
                    Longitude = attendance.Longitude,
                    SSID = attendance.SSID,
                    BSSID = attendance.BSSID,
                    IsMockLocation = attendance.IsMockLocation,
                    Action = attendance.Action
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // نفس الدالة برضه
                var response = await LoginPage.PostWithTokenAsync(
                    "https://elnagarygroup-001-site1.ktempurl.com/api/AttendancePolicy/verify",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ✅ تنفيذ VerifyOfflineAsync
       

    }
}
