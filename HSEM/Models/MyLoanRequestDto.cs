using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HSEM.Models
{
    public class MyLoanRequestDto
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public int Installments { get; set; }
        public int PaidInstallments { get; set; }
        public int RemainingInstallments { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public decimal Remaining { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime Date { get; set; }

        // خصائص العرض
        public string StatusText => IsCompleted ? "تم السداد" : "قيد السداد";
        public string RemainingText => $"{Remaining:0.##} جنيه";
        public string AmountText => $"{Amount:0.##} جنيه";
        public string RemainingInstallmentsText => $"{RemainingInstallments} قسط متبقي";
        public string DateText => Date.ToLocalTime().ToString("yyyy/MM/dd");
        // 👇 اللون حسب الحالة
        public Color StatusColor => IsCompleted ? Colors.Green : Colors.DarkRed;
    }
}
