using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class MissionRequestDto
    {
        public DateTime MissionDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
    }
}
