using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class PermissionRequestDto
    {
        public PermissionType Type { get; set; }
        public PermissionScope Scope { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? ManagerNotes { get; set; }
    }

}
