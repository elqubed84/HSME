using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public static class OfflineAttendanceQueue
    {
        private const string INDEX_KEY = "ATT_INDEX";

        public static async Task EnqueueAsync(OfflineAttendance item)
        {
            var json = JsonSerializer.Serialize(item);
            await SecureStorage.SetAsync(item.Id.ToString(), json);

            var index = await GetIndexAsync();
            index.Add(item.Id.ToString());
            await SaveIndexAsync(index);
        }

        public static async Task<List<OfflineAttendance>> GetAllAsync()
        {
            var result = new List<OfflineAttendance>();
            var index = await GetIndexAsync();

            foreach (var key in index)
            {
                var json = await SecureStorage.GetAsync(key);
                if (json != null)
                {
                    result.Add(JsonSerializer.Deserialize<OfflineAttendance>(json)!);
                }
            }

            return result;
        }

        public static async Task RemoveAsync(Guid id)
        {
            SecureStorage.Remove(id.ToString());

            var index = await GetIndexAsync();
            index.Remove(id.ToString());
            await SaveIndexAsync(index);
        }

        private static async Task<List<string>> GetIndexAsync()
        {
            var json = await SecureStorage.GetAsync(INDEX_KEY);
            return json == null
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(json)!;
        }

        private static async Task SaveIndexAsync(List<string> index)
        {
            await SecureStorage.SetAsync(INDEX_KEY, JsonSerializer.Serialize(index));
        }
    }
}
