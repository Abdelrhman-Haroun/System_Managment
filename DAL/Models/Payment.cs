using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Payment : Base
    {
        public int? CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        public int? SupplierId { get; set; }
        [ForeignKey(nameof(SupplierId))]
        public Supplier Supplier { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public bool IsIncoming { get; set; } = true; // true = received, false = paid

        [Required]
        public PaymentMethodType PaymentMethod { get; set; }

        public int? BankAccountId { get; set; }
        [ForeignKey(nameof(BankAccountId))]
        public BankAccount BankAccount { get; set; }

        public int? CashboxId { get; set; }
        [ForeignKey(nameof(CashboxId))]
        public CashBox CashBox { get; set; }

        public int? MobileWalletId { get; set; }
        [ForeignKey(nameof(MobileWalletId))]
        public MobileWallet MobileWallet { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
