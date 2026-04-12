using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Android.Runtime;
using Microsoft.Maui.Controls;

namespace HSEM.Models
{
    [Preserve(AllMembers = true)]
    public class EmployeeEvaluationItemDto
    {
        [JsonPropertyName("criteriaName")]
        public string CriteriaName { get; set; } = string.Empty;

        [JsonPropertyName("maxScore")]
        public int MaxScore { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        public string? Notes { get; set; }
        public IEnumerable<int> Stars => Enumerable.Range(1, MaxScore);
    }
}
