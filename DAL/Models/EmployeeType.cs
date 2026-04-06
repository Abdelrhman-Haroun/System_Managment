using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class EmployeeType : Base
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}
