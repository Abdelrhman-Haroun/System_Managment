using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models 
{
    public class Employee : Base
    {
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public int? EmployeeTypeId { get; set; }
        public EmployeeType? EmployeeType { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Today;

        [StringLength(100)]
        public string? Position { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<EmployeeAttendance>? Attendances { get; set; }
        public ICollection<EmployeeSalaryAdjustment>? SalaryAdjustments { get; set; }
    }
}
