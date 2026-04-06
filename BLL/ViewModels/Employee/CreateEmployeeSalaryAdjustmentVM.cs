using System.ComponentModel.DataAnnotations;
using DAL.Models;

namespace BLL.ViewModels.Employee
{
    public class CreateEmployeeSalaryAdjustmentVM
    {
        [Required(ErrorMessage = "الموظف مطلوب")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        public DateTime AdjustmentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "نوع الحركة مطلوب")]
        public string AdjustmentType { get; set; } = SalaryAdjustmentTypes.Addition;

        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "السبب مطلوب")]
        [StringLength(150)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
