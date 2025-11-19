using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class SalesInvoice : Base
    {
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("InvoiceItem")]
        public int InvoiceItemId { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } // get from InvoiceItem total of all products

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }


        // Navigation Properties
        public virtual Customer Customer { get; set; }
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }

}
