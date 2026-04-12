using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public class AttendanceSyncService
    {
        private readonly HttpClient _httpClient;

        public AttendanceSyncService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SyncAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return;

            var records = await OfflineAttendanceQueue.GetAllAsync();

            foreach (var item in records)
            {
                try
                {
                    var json = JsonSerializer.Serialize(item);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("attendance/checkin", content);
                    if (response.IsSuccessStatusCode)
                    {
                        await OfflineAttendanceQueue.RemoveAsync(item.Id);
                    }
                }
                catch
                {
                    // تجاهل ونكمل
                }
            }
        }
    }
}
