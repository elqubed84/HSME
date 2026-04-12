using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface ILoanQueryService
    {
        Task<List<MyLoanRequestDto>> GetMyRequestsAsync(string token);
    }

}
