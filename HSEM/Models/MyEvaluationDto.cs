using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
#if ANDROID
using Android.Runtime;
#endif


namespace HSEM.Models
{
    public class MyEvaluationDto
    {
        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("monthText")]
        public string MonthText { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<EmployeeEvaluationItemDto> Items { get; set; } = new();
    }

}
