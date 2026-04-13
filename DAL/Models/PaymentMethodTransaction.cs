using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class PaymentMethodTransaction : Base
    {
        public int PaymentId { get; set; }
        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; }

        public int? CashboxId { get; set; }
        [ForeignKey(nameof(CashboxId))]
        public virtual CashBox CashBox { get; set; }

        public int? BankAccountId { get; set; }
        [ForeignKey(nameof(BankAccountId))]
        public virtual BankAccount BankAccount { get; set; }

        public int? MobileWalletId { get; set; }
        [ForeignKey(nameof(MobileWalletId))]
        public virtual MobileWallet MobileWallet { get; set; }

        [Required, StringLength(100)]
        public string SourceType { get; set; } // Cashbox, BankAccount, MobileWallet

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountChanged { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string ReferenceNumber { get; set; } = string.Empty;
    }
}
