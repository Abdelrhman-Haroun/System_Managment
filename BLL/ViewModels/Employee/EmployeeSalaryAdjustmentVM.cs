namespace BLL.ViewModels.Employee
{
    public class EmployeeSalaryAdjustmentVM
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
