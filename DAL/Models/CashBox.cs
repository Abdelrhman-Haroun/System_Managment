using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class CashBox
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الخزينة مطلوب")]
        [StringLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Balance { get; set; } = 0; // Positive = they owe us


        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Transaction> Transactions { get; set; }
    }

}
