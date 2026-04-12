using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface IPermissionRequestService
    {
        Task<(bool success, string message)> SubmitAsync(PermissionRequestDto dto, string token);
    }

}
