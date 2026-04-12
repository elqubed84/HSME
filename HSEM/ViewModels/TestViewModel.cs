using HSEM.Models;
using System.Collections.ObjectModel;

namespace HSEM.ViewModels
{
    public class TestViewModel
    {
        public ObservableCollection<LeaveRequest> Requests { get; set; }

        public TestViewModel()
        {
            Requests = new ObservableCollection<LeaveRequest>
            {
                new LeaveRequest { Date="2025-11-01", DaysCount=3, Status="مقبولة" },
                new LeaveRequest { Date="2025-10-20", DaysCount=2, Status="قيد الانتظار" },
                new LeaveRequest { Date="2025-09-18", DaysCount=1, Status="مرفوضة" },
                new LeaveRequest { Date="2025-08-30", DaysCount=4, Status="مقبولة" },
                new LeaveRequest { Date="2025-07-10", DaysCount=2, Status="قيد الانتظار" }
            };
        }
    }
}
