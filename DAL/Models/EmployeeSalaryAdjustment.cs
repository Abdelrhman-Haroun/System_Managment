using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class EmployeeSalaryAdjustment : Base
    {
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime AdjustmentDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(20)]
        public string AdjustmentType { get; set; } = SalaryAdjustmentTypes.Addition;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(150)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public static class SalaryAdjustmentTypes
    {
        public const string Addition = "Addition";
        public const string Deduction = "Deduction";
    }
}
