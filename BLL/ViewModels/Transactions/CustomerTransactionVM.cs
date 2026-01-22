using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Transactions
{
    public class CustomerTransactionVM
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal BalanceBefore { get; set; }
        public decimal AmountChanged { get; set; }  // Changed from Amount to AmountChanged
        public decimal BalanceAfter { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;  // Changed from Notes to Description
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerDebtSummaryVM
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
        public int InvoiceCount { get; set; }
    }
}
