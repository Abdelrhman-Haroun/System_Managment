using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Employee
{
    public class EditEmployeeVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الموظف مطلوب")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "الراتب يجب أن يكون أكبر من صفر")]
        public decimal Salary { get; set; }

        public int? EmployeeTypeId { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [Required(ErrorMessage = "تاريخ التعيين مطلوب")]
        public DateTime HireDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; }
    }
}
