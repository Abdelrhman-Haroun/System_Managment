namespace BLL.ViewModels.Payment
{
    public class PaymentBalanceVM
    {
        public string PartyType { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string? ReasonHint { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalPayments { get; set; }
        public int PaymentsCount { get; set; }
        public string? TransactionsUrl { get; set; }
    }
}
