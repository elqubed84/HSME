using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class LatestRequestModel
    {
        public int LeaveRequestId { get; set; }
        public string EmployeeId { get; set; }
        public string LeaveType { get; set; }
        public string Reason { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int DaysCount { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
    }

}
