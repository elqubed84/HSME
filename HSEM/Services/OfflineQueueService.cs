using HSEM.Helper;
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
    public class OfflineQueueService : IOfflineQueueService
    {
        private const string Key = "PendingPermissionRequests";

        public async Task EnqueueAsync(PermissionRequestDto dto)
        {
            var list = await LoadAsync();
            list.Add(new PendingPermissionRequest { Request = dto, CreatedAt = DateTime.UtcNow });
            await SaveAsync(list);
        }

        public async Task<List<PendingPermissionRequest>> DequeueAllAsync()
        {
            return await LoadAsync();
        }

        public async Task ClearAsync()
        {
            await SaveAsync(new List<PendingPermissionRequest>());
        }

        private async Task<List<PendingPermissionRequest>> LoadAsync()
        {
            var json = Preferences.Get(Key, "[]");
            return JsonSerializer.Deserialize<List<PendingPermissionRequest>>(json) ?? new();
        }

        private async Task SaveAsync(List<PendingPermissionRequest> list)
        {
            var json = JsonSerializer.Serialize(list);
            Preferences.Set(Key, json);
            await Task.CompletedTask;
        }
    }

}
