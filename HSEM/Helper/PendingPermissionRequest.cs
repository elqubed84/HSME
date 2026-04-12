using HSEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Helper
{
    public class PendingPermissionRequest
    {
        public PermissionRequestDto Request { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
