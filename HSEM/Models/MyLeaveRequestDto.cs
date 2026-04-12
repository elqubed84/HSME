using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class MyLeaveRequestDto
    {
      
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysCount { get; set; }
        public string Status { get; set; }
       
        public DateTime CreatedAt { get; set; }

        public string CreatedAtDate => CreatedAt.ToString("yyyy-MM-dd");
        public string Date => $"{EndDate:yyyy-MM-dd} → {StartDate:yyyy-MM-dd}";
    }

}
