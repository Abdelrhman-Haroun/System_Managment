using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class EmployeeSalaryHistory : Base
    {
        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    }
}