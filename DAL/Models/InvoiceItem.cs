using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class InvoiceItem : Base
    {
        public int Quantity { get; set; } = 1; 
        public decimal Weight { get; set; } = 1;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int? ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        public int? SaleInvoiceId { get; set; }
        [ForeignKey(nameof(SaleInvoiceId))]
        public SalesInvoice SaleInvoice { get; set; }

        public int? PurchaseInvoiceId { get; set; }
        [ForeignKey(nameof(PurchaseInvoiceId))]
        public PurchaseInvoice PurchaseInvoice { get; set; }
    }

}
