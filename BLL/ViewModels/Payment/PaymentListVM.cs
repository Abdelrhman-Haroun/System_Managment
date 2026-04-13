using DAL.Models;

namespace BLL.ViewModels.Payment
{
    public class PaymentListVM
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public bool IsIncoming { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PartyType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string PaymentSourceName { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Notes { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? RelatedTransactionsUrl { get; set; }
    }
}
