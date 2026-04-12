using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface IAttendanceService
    {
        Task<bool> VerifyOnlineAsync(
            double lat,
            double lng,
            string ssid,
            string bssid,
            bool isMock,
            string action);

        Task<bool> VerifyOfflineAsync(OfflineAttendance attendance);
    }
}
