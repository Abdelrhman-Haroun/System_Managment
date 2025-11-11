using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models 
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الموظف مطلوب")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "الراتب يجب أن يكون أكبر من صفر")]
        public decimal Salary { get; set; }

        [StringLength(100)]
        public string Position { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
