using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface INavigationService
    {
        Task GoToAttendanceDetailsAsync(int year, int month);
        Task GoToPayrollDetailsAsync(int year, int month,string monthname);
    }

}
