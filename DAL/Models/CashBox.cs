using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class CashBox : Base
    {
        [StringLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Balance { get; set; } = 0; // Positive = they owe us

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

}
