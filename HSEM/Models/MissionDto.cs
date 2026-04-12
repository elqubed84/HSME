using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class MissionDto
    {
        public int Id { get; set; }
        public DateTime MissionDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public string MissionDateText => MissionDate.ToString("yyyy-MM-dd");
        public string TimeRange => $"{FromTime:hh\\:mm} - {ToTime:hh\\:mm}";
    }
}
