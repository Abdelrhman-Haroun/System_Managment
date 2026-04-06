using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class EmployeeAttendance : Base
    {
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = EmployeeAttendanceStatus.Present;

        [StringLength(250)]
        public string? Notes { get; set; }
    }

    public static class EmployeeAttendanceStatus
    {
        public const string Present = "Present";
        public const string Absent = "Absent";
    }
}
