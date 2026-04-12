using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System;
    using System.Text.Json.Serialization;


namespace HSEM.Models
{
    public class MonthlyPayroll
    {
        public string EmployeeName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string MonthName { get; set; } = "";

        public int Year { get; set; }
        public int Month { get; set; }

        // المرتب
        public decimal BasicSalary { get; set; }
        public int AbsentDays { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal BaseAfterAbsence { get; set; }

        public decimal Allowances { get; set; }
        public decimal ExtraHoursAmount { get; set; }
        public decimal Bonus { get; set; }

        public decimal Deductions { get; set; }
        public decimal Loans { get; set; }
        public decimal InsuranceEmployee { get; set; }
        public decimal InsuranceCompany { get; set; }
        public decimal IncomeTax { get; set; }

        public decimal NetSalary { get; set; }
        public DateTime CreatedAt { get; set; }

        // السلف
        public List<Loan> LoansList { get; set; } = new();
        public List<Penalty> PenaltiesList { get; set; } = new List<Penalty>();
    }
    // Loan (mobile)
    public class Loan
    {
        public int LoanId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int Installments { get; set; }
        public int PaidInstallments { get; set; }
        public int RemainingInstallments { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public decimal Remaining { get; set; }
        public bool IsCompleted { get; set; } = false;
        public decimal? PaymentAmount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
    }
}
