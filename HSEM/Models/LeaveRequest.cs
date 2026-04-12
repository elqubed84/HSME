using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class LeaveRequest
    {

        public string Date { get; set; }
        public int DaysCount { get; set; }
        public string Status { get; set; }
    
    //public LeaveRequest(string date, int daysCount, string status)
    //    {
    //        Date = date;
    //        DaysCount = daysCount;
    //        Status = status;
    //    }
    }
}
