using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class Penalty
    {
        [Key]
        public int PenaltyId { get; set; }

        public string UserId { get; set; } = string.Empty;


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Now;

        public string Type { get; set; } = string.Empty;

        public decimal? DaysCount { get; set; } = 0m;

        public string? Notes { get; set; }

        public decimal DeductionValue { get; set; } = 0m;

        public string? CreatedBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool AppliedToPayroll { get; set; } = false;
    }
}
