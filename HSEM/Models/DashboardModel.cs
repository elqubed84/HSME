using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class DashboardModel
    {
        public string FullName { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public int Balance { get; set; }
        public int AbsenceDays { get; set; }
        public IEnumerable<MonthStat> MonthlyStats => MonthlyData.Select((v, i) => new MonthStat { Month = i + 1, Count = v });
        public int[] MonthlyData { get; set; }
        public IEnumerable<LeaveItem> LatestRequests { get; set; }
    }

    public class MonthStat { public int Month { get; set; } public int Count { get; set; } }
    public class LeaveItem { public string Date { get; set; } public int DaysCount { get; set; } public string Status { get; set; } }

}
