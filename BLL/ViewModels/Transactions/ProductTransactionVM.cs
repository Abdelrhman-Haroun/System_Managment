using System;
using DAL.Models;

namespace BLL.ViewModels.Transactions
{
    public class ProductTransactionVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int InvoiceId { get; set; }
        public int ProductType { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal QuantityBefore { get; set; }
        public decimal QuantityChanged { get; set; }
        public decimal WeightChanged { get; set; }
        public decimal QuantityAfter { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DisplayTotalAmount => TransactionTypes.IsInternalUsage(TransactionType) ? WeightChanged * UnitPrice : TotalAmount;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
