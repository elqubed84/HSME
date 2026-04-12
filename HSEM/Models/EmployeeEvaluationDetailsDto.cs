using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HSEM.Models
{
    [Preserve(AllMembers = true)]
    public class EvaluationDetailsDto
    {
        [JsonPropertyName("employeeName")]
        public string? EmployeeName { get; set; }

        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("averageScore")]
        public decimal AverageScore { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("items")]
        public List<EmployeeEvaluationItemDto> Items { get; set; } = new();
    }
}
