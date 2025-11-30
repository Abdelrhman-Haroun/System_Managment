using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DAL.Models;

namespace DAL.Models
{
    public class Customer : Base
    {
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Balance { get; set; } = 0; // Positive = they owe us

        // Navigation Properties
        public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

}
