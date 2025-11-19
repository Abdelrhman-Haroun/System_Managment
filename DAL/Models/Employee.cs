using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models 
{
    public class Employee : Base
    {
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }
    }
}
