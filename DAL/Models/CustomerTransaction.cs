using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class CustomerTransaction : Base
    {
        [Required]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [Required]
        [ForeignKey(nameof(Invoice))]
        public int InvoiceId { get; set; }

        // Transaction type: Invoice, Payment, Adjustment
        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; }

        // Balance changes
        public decimal BalanceBefore { get; set; }
        public decimal AmountChanged { get; set; }
        public decimal BalanceAfter { get; set; }

        // Details
        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        // Navigation Properties
        public virtual Customer Customer { get; set; }
        public virtual Invoice Invoice { get; set; }
    }
}
