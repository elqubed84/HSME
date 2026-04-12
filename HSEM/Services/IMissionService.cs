using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public interface IMissionService
    {
        Task<(bool success, string message)> SubmitAsync(
            MissionRequestDto dto,
            string token);
    }
}
