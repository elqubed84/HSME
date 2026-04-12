using HSEM.Helper;
using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface IOfflineQueueService
    {
        Task EnqueueAsync(PermissionRequestDto dto);
        Task<List<PendingPermissionRequest>> DequeueAllAsync();
        Task ClearAsync();
    }

}
