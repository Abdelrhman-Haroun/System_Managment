using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class EmployeeTransaction : Base
    {
        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }

        public int? InvoiceId { get; set; }
        [ForeignKey(nameof(InvoiceId))]
        public Invoice? Invoice { get; set; }

        [Required, StringLength(50)]
        public string TransactionType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountChanged { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
