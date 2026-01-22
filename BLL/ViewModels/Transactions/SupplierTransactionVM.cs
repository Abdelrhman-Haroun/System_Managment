using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Transactions
{
    public class SupplierTransactionVM
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int InvoiceId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal BalanceBefore { get; set; }
        public decimal AmountChanged { get; set; }  // Changed from Amount to AmountChanged
        public decimal BalanceAfter { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;  // Changed from Notes to Description
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SupplierCreditSummaryVM
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalCredit { get; set; }
        public int InvoiceCount { get; set; }
    }
}
