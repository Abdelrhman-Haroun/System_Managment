using DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ProductTransaction : Base
    {
        [Required]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required]
        [ForeignKey(nameof(Invoice))]
        public int InvoiceId { get; set; }

        // Transaction type: Purchase, Sales, Adjustment
        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; }

        public int? ProductType { get; set; }

        // Quantities
        public decimal QuantityBefore { get; set; }
        public decimal QuantityChanged { get; set; }
        public decimal WeightChanged { get; set; }
        public decimal QuantityAfter { get; set; }

        // Prices
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }

        // Metadata
        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // Navigation Properties
        public virtual Product Product { get; set; }
        public virtual Invoice Invoice { get; set; }
    }
}
