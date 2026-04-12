using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface ILeaveRequestQueryService
    {
        Task<List<MyLeaveRequestDto>> GetMyLeaveRequestsAsync(string token);
    }

}
