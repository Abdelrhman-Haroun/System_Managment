namespace BLL.ViewModels.Employee
{
    public class EmployeeMonthlyPayrollVM
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeTypeName { get; set; } = "غير محدد";
        public string? Position { get; set; }
        public decimal Salary { get; set; }
        public decimal DailyRate { get; set; }
        public decimal EarnedSalary { get; set; }
        public int DaysInMonth { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int PaidOffDaysAllowance { get; set; }
        public int ExtraWorkedOffDays { get; set; }
        public int UnpaidAbsentDays { get; set; }
        public decimal OffDaysCompensation { get; set; }
        public decimal AbsenceDeduction { get; set; }
        public decimal Additions { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
    }
}
