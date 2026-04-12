using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class DashboardData
    {
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public int Age { get; set; }
        public string WorkDuration { get; set; }
        public string Notes { get; set; }
        public int Accepted { get; set; }
        public int Rejected { get; set; }
        public int Pending { get; set; }
        public int Balance { get; set; }
        public int AbsenceDays { get; set; }
        public int[] MonthlyData { get; set; }
        public List<LatestRequestModel> LatestRequests { get; set; }
    }

}
