using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Supplier : Base
    {
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Balance { get; set; } = 0; 

        // Navigation Properties
        public virtual ICollection<Invoice> PurchaseInvoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
