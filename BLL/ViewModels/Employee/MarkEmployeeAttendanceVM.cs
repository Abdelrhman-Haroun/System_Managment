using System.ComponentModel.DataAnnotations;
using DAL.Models;

namespace BLL.ViewModels.Employee
{
    public class MarkEmployeeAttendanceVM
    {
        [Required(ErrorMessage = "الموظف مطلوب")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "الحالة مطلوبة")]
        public string Status { get; set; } = EmployeeAttendanceStatus.Present;

        [StringLength(250)]
        public string? Notes { get; set; }
    }
}
