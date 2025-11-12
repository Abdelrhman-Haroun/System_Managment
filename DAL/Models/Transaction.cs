using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionNumber { get; set; }

        [Required]
        [StringLength(30)]
        public int Type { get; set; } // Sale 1, Purchase 2

        // Product (for Sale/Purchase)
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        // Parties
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Quantity * UnitPrice

        // Payment Tracking
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }

        // Where the money goes based on payment type
        [ForeignKey("Cashbox")]
        public int CashboxId { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Product Product { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual CashBox CashBox { get; set; }
    }

}
