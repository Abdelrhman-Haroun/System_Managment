using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Invoice : Base
    {
        [Required]
        [StringLength(50)]
        public string InvoiceType { get; set; } 
        public int? CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer? Customer { get; set; }
        public int? SupplierId { get; set; }
        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier? Supplier { get; set; }
        [StringLength(50)]
        public string? InvoiceNumber { get; set; }
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }
        [Required]
        public DateTime InvoiceDate { get; set; } 
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        [NotMapped]
        public bool IsPurchase => InvoiceType == InvoiceTypes.Purchase;

        [NotMapped]
        public bool IsSales => InvoiceType == InvoiceTypes.Sales;

        [NotMapped]
        public string? CustomerOrSupplierName => IsSales ? Customer?.Name : Supplier?.Name;
    }

    public static class InvoiceTypes
    {
        public const string Purchase = "Purchase";
        public const string Sales = "Sales";
    }
}

